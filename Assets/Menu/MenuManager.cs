using UnityEngine;
using UnityEngine.SceneManagement;
using GameMeta;

public class MenuManager : MonoBehaviour
{
    [Header("目标游戏场景")]
    public string gameSceneName = "GameScene";

    int highScore;
    bool newGameConfirm;   // "开始新游戏"确认框

    ShopManager shop;
    AchievementPanel achievements;
    SaveSlotPanel savePanel;

    // 固定按钮顺序：开始新游戏 / 继续游戏 / 存档 / 升级商店 / 成就 / 退出
    const int BtnCount = 6;

    void Start()
    {
        Time.timeScale = 1;
        MetaStore.Load();
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        shop = FindObjectOfType<ShopManager>(true);
        achievements = FindObjectOfType<AchievementPanel>(true);
        savePanel = FindObjectOfType<SaveSlotPanel>(true);
    }

    void OnGUI()
    {
        // 子面板/加载遮罩打开时主菜单不响应
        if (shop != null && shop.IsOpen) return;
        if (achievements != null && achievements.IsOpen) return;
        if (savePanel != null && savePanel.IsOpen) return;

        // 响应式布局
        float btnW = Mathf.Min(280f, Screen.width * 0.28f);
        float btnH = Mathf.Min(52f, Screen.height * 0.07f);
        float gap = Mathf.Min(14f, Screen.height * 0.02f);
        float totalH = BtnCount * btnH + (BtnCount - 1) * gap;
        float cw = Screen.width / 2f;
        float startY = Screen.height * 0.32f;
        if (startY + totalH > Screen.height - 20)
            startY = Screen.height - totalH - 20;

        // 标题与最高分
        GUI.Label(new Rect(0, Screen.height * 0.08f, Screen.width, 70), "霓虹幸存者",
            MenuUI.Label(46));
        GUI.Label(new Rect(0, Screen.height * 0.08f + 78, Screen.width, 32),
            "历史最高分：" + highScore, MenuUI.Label(20));

        // 新游戏确认框为模态：打开期间不绘制底层按钮
        if (newGameConfirm)
        {
            DrawNewGameConfirm();
            return;
        }

        float y = startY;

        // 1. 开始新游戏
        if (MenuUI.Button(new Rect(cw - btnW / 2f, y, btnW, btnH), "开始新游戏", 20))
            newGameConfirm = true;
        y += btnH + gap;

        // 2. 继续游戏（无存档灰显并提示）
        int latest = SaveManager.LatestSlotIndex();
        Rect continueRect = new Rect(cw - btnW / 2f, y, btnW, btnH);
        if (latest >= 0)
        {
            if (MenuUI.Button(continueRect, "继续游戏", 20))
                LoadSlot(latest);
        }
        else
        {
            MenuUI.DisabledButton(continueRect, "继续游戏", "暂无存档记录，请先开始新游戏", 20);
        }
        y += btnH + gap;

        // 3. 存档（读档管理列表）
        if (MenuUI.Button(new Rect(cw - btnW / 2f, y, btnW, btnH), "存档", 20))
            savePanel?.OpenLoad();
        y += btnH + gap;

        // 4. 升级商店
        if (MenuUI.Button(new Rect(cw - btnW / 2f, y, btnW, btnH), "升级商店", 20))
            shop?.Open();
        y += btnH + gap;

        // 5. 成就
        if (MenuUI.Button(new Rect(cw - btnW / 2f, y, btnW, btnH), "成就", 20))
            achievements?.Open();
        y += btnH + gap;

        // 6. 退出
        if (MenuUI.Button(new Rect(cw - btnW / 2f, y, btnW, btnH), "退出", 20))
            Application.Quit();
    }

    void DrawNewGameConfirm()
    {
        MenuUI.Dim();
        float w = Mathf.Min(420f, Screen.width * 0.5f);
        var area = new Rect(Screen.width / 2f - w / 2f, Screen.height / 2f - 100, w, 200);
        GUI.Box(area, "");

        GUI.Label(new Rect(area.x, area.y + 26, area.width, 30), "开始新游戏", MenuUI.Label(22));
        GUI.Label(new Rect(area.x + 20, area.y + 70, area.width - 40, 50),
            "此操作将开始新游戏，不会影响现有存档", MenuUI.Label(16));

        if (MenuUI.Button(new Rect(area.x + 30, area.yMax - 58, (w - 80) / 2f, 40), "确认"))
        {
            newGameConfirm = false;
            StartNewGame();
        }
        if (MenuUI.Button(new Rect(area.xMax - 30 - (w - 80) / 2f, area.yMax - 58,
                                  (w - 80) / 2f, 40), "取消"))
        {
            newGameConfirm = false;
        }
    }

    void LoadSlot(int slot)
    {
        // 由 SaveSlotPanel 显示加载进度并负责跳转
        if (savePanel != null)
        {
            savePanel.OpenLoad();
            savePanel.StartLoadPublic(slot);
        }
        else
        {
            RunBootstrap.SlotToLoad = slot;
            SceneManager.LoadScene(gameSceneName);
        }
    }

    void StartNewGame()
    {
        RunBootstrap.SlotToLoad = -1;   // 新游戏不读档，不触碰任何槽位
        SceneManager.LoadScene(gameSceneName);
    }
}
