using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameMeta;

/// <summary>
/// 存档槽位面板（IMGUI），两种场景共用：
/// - 主菜单 Load 模式：存档列表（时间倒序）→ 点击直接加载（进度提示 + 错误提示）
/// - 游戏内 Save 模式：保存为新存档 / 覆盖旧存档（覆盖需二次确认）
/// 由 UIBootstrap 在 MainMenu 与 GameScene 自动挂载。
/// </summary>
public class SaveSlotPanel : MonoBehaviour
{
    public static SaveSlotPanel Instance { get; private set; }

    enum Mode { Closed, Load, SaveChoose, SaveOverwrite }

    Mode mode = Mode.Closed;
    Vector2 scroll;
    int selectedSlot = -1;       // 当前选中/待确认覆盖的槽位
    string resultMessage;        // 保存结果提示
    float resultTimer;

    // 加载进度遮罩
    bool loading;
    float loadProgress;
    int pendingSlot = -1;
    string loadError;

    public bool IsOpen => mode != Mode.Closed;

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }

    void Update()
    {
        if (resultTimer > 0f) resultTimer -= Time.unscaledDeltaTime;
    }

    // ===== 对外入口 =====
    public void OpenLoad()
    {
        mode = Mode.Load;
        selectedSlot = -1;
        resultMessage = null;
    }

    public void OpenSaveChoose()
    {
        mode = Mode.SaveChoose;
        selectedSlot = -1;
        resultMessage = null;
    }

    /// <summary>"继续游戏"直接加载最新槽位（列表已打开时调用）</summary>
    public void StartLoadPublic(int slot) => StartLoad(slot);

    public void Close()
    {
        mode = Mode.Closed;
        selectedSlot = -1;
    }

    // ===== 绘制 =====
    void OnGUI()
    {
        if (mode == Mode.Closed) return;

        switch (mode)
        {
            case Mode.Load: DrawLoad(); break;
            case Mode.SaveChoose: DrawSaveChoose(); break;
            case Mode.SaveOverwrite: DrawSaveOverwrite(); break;
        }

        // 覆盖确认（游戏内）
        if (selectedSlot >= 0 && (mode == Mode.SaveOverwrite))
            DrawOverwriteConfirm();

        // 加载进度 / 错误遮罩（主菜单）
        if (loading) DrawLoading();
    }

    // ===== 主菜单：读档列表 =====
    void DrawLoad()
    {
        MenuUI.Dim();
        float panelW = Mathf.Min(620f, Screen.width * 0.7f);
        float panelH = Mathf.Min(560f, Screen.height * 0.8f);
        var area = new Rect((Screen.width - panelW) / 2f, (Screen.height - panelH) / 2f, panelW, panelH);

        GUI.Box(area, "");
        GUI.Label(new Rect(area.x + 20, area.y + 14, 300, 30), "存档", MenuUI.Label(24, TextAnchor.MiddleLeft));

        var list = SaveManager.ListSlotIndices();
        Rect view = new Rect(area.x + 16, area.y + 56, panelW - 32, panelH - 120);
        float rowH = 64, rowGap = 8;
        float contentH = Mathf.Max(view.height, list.Count * (rowH + rowGap) + 10);

        if (list.Count == 0)
        {
            GUI.Label(view, "暂无存档记录", MenuUI.Label(18));
        }
        else
        {
            // 加载进度期间禁用列表交互，防止重复触发加载
            GUI.enabled = !loading;
            scroll = GUI.BeginScrollView(view, scroll, new Rect(0, 0, view.width - 18, contentH));
            for (int i = 0; i < list.Count; i++)
            {
                DrawLoadRow(new Rect(0, i * (rowH + rowGap), view.width - 18, rowH), list[i]);
            }
            GUI.EndScrollView();
            GUI.enabled = true;
        }

        GUI.enabled = !loading;
        if (MenuUI.Button(new Rect(area.center.x - 80, area.yMax - 52, 160, 36), "返回"))
            Close();
        GUI.enabled = true;
    }

    void DrawLoadRow(Rect r, int slotIndex)
    {
        var result = SaveManager.LoadSlot(slotIndex);
        bool hover = r.Contains(Event.current.mousePosition);

        var prevBg = GUI.backgroundColor;
        GUI.backgroundColor = hover ? new Color(0.8f, 0.9f, 1f) : new Color(0.92f, 0.92f, 0.92f);

        if (!result.success)
        {
            // 损坏的槽位也显示，但标记异常，不允许加载
            GUI.Box(r, "");
            GUI.backgroundColor = prevBg;
            GUI.Label(new Rect(r.x + 14, r.y, r.width - 28, r.height),
                SaveManager.SlotName(slotIndex) + "　【存档已损坏】",
                MenuUI.Label(16, TextAnchor.MiddleLeft));
            return;
        }

        var d = result.data;
        string label = SaveManager.SlotName(slotIndex) +
                       "\n" + MenuUI.FormatTime(d.saveUnixTime) +
                       "    第" + d.stage + "关  Lv." + d.level +
                       "    " + MenuUI.FormatSize(d.payloadBytes);

        if (GUI.Button(r, label, new GUIStyle(GUI.skin.button)
        {
            fontSize = MenuUI.ScaledFont(15),
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true
        }))
        {
            StartLoad(slotIndex);
        }
        GUI.backgroundColor = prevBg;
    }

    // ===== 游戏内：保存选项 =====
    void DrawSaveChoose()
    {
        MenuUI.Dim();
        var area = new Rect(Screen.width / 2f - 160, Screen.height / 2f - 150, 320, 300);
        GUI.Box(area, "");
        GUI.Label(new Rect(area.x, area.y + 16, area.width, 30), "保存存档", MenuUI.Label(22));

        var br = new Rect(area.x + 40, area.y + 70, area.width - 80, 42);
        if (MenuUI.Button(br, "保存为新存档")) SaveAsNew();
        if (MenuUI.Button(new Rect(br.x, br.y + 54, br.width, 42), "覆盖旧存档"))
            mode = Mode.SaveOverwrite;
        if (MenuUI.Button(new Rect(br.x, br.y + 108, br.width, 42), "返回"))
            Close();

        if (resultTimer > 0f && !string.IsNullOrEmpty(resultMessage))
            GUI.Label(new Rect(area.x, area.yMax - 40, area.width, 24), resultMessage, MenuUI.Label(14));
    }

    void DrawSaveOverwrite()
    {
        MenuUI.Dim();
        float panelW = Mathf.Min(560f, Screen.width * 0.7f);
        float panelH = Mathf.Min(480f, Screen.height * 0.75f);
        var area = new Rect((Screen.width - panelW) / 2f, (Screen.height - panelH) / 2f, panelW, panelH);

        GUI.Box(area, "");
        GUI.Label(new Rect(area.x + 20, area.y + 14, 400, 30), "选择要覆盖的存档", MenuUI.Label(22, TextAnchor.MiddleLeft));

        var list = SaveManager.ListSlotIndices();
        Rect view = new Rect(area.x + 16, area.y + 54, panelW - 32, panelH - 118);
        float rowH = 56, rowGap = 8;

        scroll = GUI.BeginScrollView(view, scroll,
            new Rect(0, 0, view.width - 18, Mathf.Max(view.height, list.Count * (rowH + rowGap) + 10)));
        for (int i = 0; i < list.Count; i++)
        {
            DrawOverwriteRow(new Rect(0, i * (rowH + rowGap), view.width - 18, rowH), list[i]);
        }
        GUI.EndScrollView();

        if (MenuUI.Button(new Rect(area.center.x - 80, area.yMax - 52, 160, 36), "返回"))
            mode = Mode.SaveChoose;
    }

    void DrawOverwriteRow(Rect r, int slotIndex)
    {
        var result = SaveManager.LoadSlot(slotIndex);
        bool hover = r.Contains(Event.current.mousePosition);
        var prev = GUI.backgroundColor;
        GUI.backgroundColor = hover ? new Color(1f, 0.85f, 0.7f) : Color.white;

        string text = result.success
            ? SaveManager.SlotName(slotIndex) + "\n" + MenuUI.FormatTime(result.data.saveUnixTime) +
              "    第" + result.data.stage + "关  Lv." + result.data.level
            : SaveManager.SlotName(slotIndex) + "（损坏）";

        if (GUI.Button(r, text, new GUIStyle(GUI.skin.button)
        {
            fontSize = MenuUI.ScaledFont(14),
            alignment = TextAnchor.MiddleLeft
        }))
        {
            selectedSlot = slotIndex;   // 弹覆盖确认
        }
        GUI.backgroundColor = prev;
    }

    void DrawOverwriteConfirm()
    {
        MenuUI.Dim(0.7f);
        var area = new Rect(Screen.width / 2f - 190, Screen.height / 2f - 90, 380, 180);
        GUI.Box(area, "");
        GUI.Label(new Rect(area.x, area.y + 22, area.width, 30),
            "确认覆盖 " + SaveManager.SlotName(selectedSlot) + " ？", MenuUI.Label(19));
        GUI.Label(new Rect(area.x, area.y + 58, area.width, 26), "原存档内容将被替换且无法恢复", MenuUI.Label(13));

        if (MenuUI.Button(new Rect(area.x + 30, area.yMax - 56, 140, 38), "确认覆盖"))
        {
            int target = selectedSlot;
            selectedSlot = -1;
            OverwriteTo(target);
        }
        if (MenuUI.Button(new Rect(area.xMax - 170, area.yMax - 56, 140, 38), "取消"))
            selectedSlot = -1;
    }

    // ===== 保存执行 =====
    bool BuildSnapshot(out RunSaveData data)
    {
        data = null;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || GameManager.Instance == null) return false;
        var exp = player.GetComponent<PlayerExp>();
        var hp = player.GetComponent<PlayerHealth>();
        if (exp == null || hp == null) return false;
        data = RunSnapshot.Build(player, GameManager.Instance, exp, hp);
        return true;
    }

    void SaveAsNew()
    {
        int slot = SaveManager.FirstEmptySlot();
        if (slot < 0)
        {
            resultMessage = "存档槽位已满（" + SaveManager.MaxSlots + "个），请选择覆盖旧存档";
            resultTimer = 3f;
            return;
        }
        WriteSlot(slot);
    }

    void OverwriteTo(int slot)
    {
        WriteSlot(slot);
        mode = Mode.SaveChoose;
    }

    void WriteSlot(int slot)
    {
        if (BuildSnapshot(out var data) && SaveManager.SaveToSlot(slot, data))
        {
            resultMessage = "已保存到 " + SaveManager.SlotName(slot);
            resultTimer = 3f;
        }
        else
        {
            resultMessage = "保存失败，请重试";
            resultTimer = 3f;
        }
    }

    // ===== 加载执行（主菜单）=====
    void StartLoad(int slot)
    {
        var check = SaveManager.LoadSlot(slot);
        if (!check.success)
        {
            pendingSlot = -1;
            loadError = check.error;
            loading = true;
            loadProgress = 1f;
            return;
        }
        pendingSlot = slot;
        loadError = null;
        loading = true;
        loadProgress = 0f;
        StartCoroutine(LoadRoutine(slot));
    }

    IEnumerator LoadRoutine(int slot)
    {
        // 真实读档是瞬时的；播放短进度条给出明确加载反馈（总时长约 0.5 秒）
        while (loadProgress < 0.99f)
        {
            loadProgress += Time.unscaledDeltaTime / 0.5f;
            yield return null;
        }

        var result = SaveManager.LoadSlot(slot);
        if (!result.success)
        {
            loadError = result.error;
            pendingSlot = -1;
            yield break;   // 停在错误遮罩，等用户确认回主菜单
        }

        RunBootstrap.SlotToLoad = slot;
        Time.timeScale = 1;
        SceneManager.LoadScene(result.data.sceneName);
    }

    void DrawLoading()
    {
        MenuUI.Dim(0.75f);
        var area = new Rect(Screen.width / 2f - 200, Screen.height / 2f - 80, 400, 160);
        GUI.Box(area, "");

        if (!string.IsNullOrEmpty(loadError))
        {
            GUI.Label(new Rect(area.x + 16, area.y + 24, area.width - 32, 50),
                "加载失败：\n" + loadError, MenuUI.Label(16));
            if (MenuUI.Button(new Rect(area.center.x - 80, area.yMax - 50, 160, 36), "返回主菜单"))
            {
                loading = false;
                loadError = null;
            }
            return;
        }

        GUI.Label(new Rect(area.x, area.y + 30, area.width, 30), "正在加载存档…", MenuUI.Label(20));
        // 进度条
        var bar = new Rect(area.x + 30, area.y + 82, area.width - 60, 18);
        GUI.Box(bar, "");
        GUI.color = new Color(0.4f, 0.7f, 1f);
        GUI.DrawTexture(new Rect(bar.x + 2, bar.y + 2, (bar.width - 4) * loadProgress, bar.height - 4),
                        Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(area.x, area.y + 104, area.width, 24),
            Mathf.RoundToInt(loadProgress * 100) + "%", MenuUI.Label(14));
    }
}
