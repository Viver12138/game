using UnityEngine;
using GameMeta;

/// <summary>
/// 游戏内设置面板（IMGUI）：右上角"设置"按钮 → 暂停 → 一级/音量二级/退出确认。
/// 由 UIBootstrap 在 GameScene 自动挂载。
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    public enum View { None, Main, Volume, QuitConfirm }

    [Header("按钮图片（可选：拖入图片后设置按钮显示该图，不拖则显示文字）")]
    public Texture2D buttonIcon;

    View view = View.None;
    float sfxVolume = 1f;
    float bgmVolume = 1f;

    // 布局常量
    const float BtnW = 220f, BtnH = 42f, Gap = 10f;

    void Start()
    {
        sfxVolume = SaveManager.GetSfxVolume();
        bgmVolume = SaveManager.GetBgmVolume();
    }

    void Update()
    {
        // 存档槽位面板打开期间，ESC 不响应，避免层级错乱
        if (SaveSlotPanel.Instance != null && SaveSlotPanel.Instance.IsOpen) return;

        // ESC：开/关面板
        if (Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance != null)
        {
            if (view == View.None) Open();
            else if (view == View.Main) BackToGame();
            else view = View.Main;   // 二级面板按 ESC 返回一级
        }
    }

    bool CanOpen()
    {
        if (GameManager.Instance == null) return false;
        if (GameManager.Instance.State != GameState.Playing) return false;
        // 升级三选一面板打开期间禁止暂停（避免双重 timeScale）
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.IsOpen) return false;
        return true;
    }

    void Open()
    {
        if (!CanOpen()) return;
        view = View.Main;
        GameManager.Instance.Pause();
    }

    void BackToGame()
    {
        view = View.None;
        if (GameManager.Instance != null) GameManager.Instance.Resume();
    }

    void OnGUI()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        // 存档槽位面板（保存为新存档/覆盖）打开时，设置面板整体让出绘制
        if (view != View.None && SaveSlotPanel.Instance != null && SaveSlotPanel.Instance.IsOpen)
            return;

        // 游戏中：右上角设置入口
        if (view == View.None)
        {
            if (gm.State == GameState.Playing && UpgradeOpenBlocked())
            {
                // 升级面板打开时不画
            }
            else if (gm.State == GameState.Playing)
            {
                var rect = new Rect(Screen.width - 110, 10, 100, 32);
                if (IconButton(rect, "设置", buttonIcon)) Open();
            }
            return;
        }

        // 暂停态：半透明遮罩
        DrawDim();

        switch (view)
        {
            case View.Main: DrawMain(); break;
            case View.Volume: DrawVolume(); break;
            case View.QuitConfirm: DrawQuitConfirm(); break;
        }
    }

    bool UpgradeOpenBlocked()
    {
        return UpgradeManager.Instance != null && UpgradeManager.Instance.IsOpen;
    }

    void DrawDim()
    {
        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    void DrawMain()
    {
        float panelH = BtnH * 5 + Gap * 6 + 40;
        var area = CenterRect(BtnW + 60, panelH);

        GUI.Box(area, "");
        Label(area, "设置", 26, 14);

        float y = area.y + 50;
        if (Button(y, "返回游戏")) BackToGame();
        if (Button(y += BtnH + Gap, "保存"))
        {
            // 进入存档选项：保存为新存档 / 覆盖旧存档（仅此处会写存档）
            if (SaveSlotPanel.Instance != null) SaveSlotPanel.Instance.OpenSaveChoose();
        }
        if (Button(y += BtnH + Gap, "音量设置  ▶")) view = View.Volume;
        if (Button(y += BtnH + Gap, "返回主菜单"))
        {
            // 正常退出：不自动存档（只有点了"保存"才会留档），也不结算货币
            GameManager.Instance.ToMainMenu();
        }
        if (Button(y += BtnH + Gap, "退出游戏")) view = View.QuitConfirm;
    }

    void DrawVolume()
    {
        float panelH = 200;
        var area = CenterRect(360, panelH);
        GUI.Box(area, "");
        Label(area, "音量设置", 22, 14);

        float x = area.x + 24, w = area.width - 48;
        float y = area.y + 56;

        GUI.Label(new Rect(x, y, 100, 24), "音效音量");
        sfxVolume = GUI.HorizontalSlider(new Rect(x + 100, y + 6, w - 100, 20), sfxVolume, 0f, 1f);

        y += 44;
        GUI.Label(new Rect(x, y, 100, 24), "背景音乐");
        bgmVolume = GUI.HorizontalSlider(new Rect(x + 100, y + 6, w - 100, 20), bgmVolume, 0f, 1f);

        // 仅持久化滑条值，暂不接实际音源
        SaveManager.SetSfxVolume(sfxVolume);
        SaveManager.SetBgmVolume(bgmVolume);

        if (GUI.Button(new Rect(area.center.x - 70, area.yMax - 46, 140, 32), "返回"))
            view = View.Main;
    }

    void DrawQuitConfirm()
    {
        var area = CenterRect(320, 160);
        GUI.Box(area, "");
        GUI.Label(new Rect(area.x, area.y + 20, area.width, 30), "确认退出游戏？", CenterLabel(20));

        if (GUI.Button(new Rect(area.x + 24, area.yMax - 56, 120, 36), "确认"))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        if (GUI.Button(new Rect(area.xMax - 144, area.yMax - 56, 120, 36), "取消"))
            view = View.Main;
    }

    // ===== 通用布局辅助 =====
    bool Button(float y, string text)
    {
        return GUI.Button(new Rect((Screen.width - BtnW) / 2f, y, BtnW, BtnH), text);
    }

    Rect CenterRect(float w, float h)
    {
        return new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
    }

    void Label(Rect area, string text, int size, float top)
    {
        GUI.Label(new Rect(area.x, area.y + top, area.width, size + 8), text, CenterLabel(size));
    }

    GUIStyle CenterLabel(int size)
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontSize = size,
            alignment = TextAnchor.MiddleCenter
        };
    }

    /// <summary>有图显示图+文字，无图纯文字（预留后续图片替换接口）</summary>
    bool IconButton(Rect rect, string text, Texture2D icon)
    {
        if (icon == null) return GUI.Button(rect, text);

        bool click = GUI.Button(rect, GUIContent.none);
        var iconRect = new Rect(rect.x + 6, rect.y + 6, rect.height - 12, rect.height - 12);
        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
        GUI.Label(new Rect(iconRect.xMax + 4, rect.y, rect.width - iconRect.width - 10, rect.height),
                  text, new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft });
        return click;
    }
}
