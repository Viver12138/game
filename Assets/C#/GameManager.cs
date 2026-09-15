using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using GameMeta;

public enum GameState
{
    Playing,   // 游戏中
    Paused,    // 设置面板暂停
    GameOver   // 游戏结束（等待重开）
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState State { get; private set; } = GameState.Playing;

    float playTime;
    public int score = 0;
    int runKills;            // 本局击杀数（=本局待结算货币）
    int earnedCurrency;      // 本局结算得到的货币（结束面板显示用）
    float scoreTimer;
    int highScore;
    float scoreScale = 1f;
    const string HighScoreKey = "HighScore";

    public float PlayTime => playTime;
    public int Score => score;
    public int RunKills => runKills;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // 从主菜单切进来 → 直接开玩
        State = GameState.Playing;
        Time.timeScale = 1;
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        MetaStore.Load();
    }

    void Update()
    {
        MetaStore.Tick(Time.unscaledDeltaTime);   // 击杀等高频变更节流落盘

        if (State == GameState.Playing)
        {
            playTime += Time.deltaTime;
            scoreTimer += Time.deltaTime;
            if (scoreTimer >= 1f) { scoreTimer -= 1f; score++; }
        }
        else if (State == GameState.GameOver)
        {
            if (Input.GetKeyDown(KeyCode.R)) Restart();
            if (Input.GetKeyDown(KeyCode.M)) ToMainMenu();
        }
    }

    // ===== 暂停 / 恢复（设置面板使用）=====
    public void Pause()
    {
        if (State != GameState.Playing) return;
        State = GameState.Paused;
        Time.timeScale = 0;
    }

    public void Resume()
    {
        if (State != GameState.Paused) return;
        State = GameState.Playing;
        Time.timeScale = 1;
    }

    /// <summary>敌人死亡唯一入口上报：计分 + 本局击杀数 + 累计击杀（成就）</summary>
    public void RegisterKill(int scoreValue)
    {
        runKills++;
        AddScore(scoreValue);
        MetaStore.RecordKill();
    }

    /// <summary>读档恢复计时/分数/击杀数（角色成长由 RunBootstrap 恢复）</summary>
    public void RestoreRunStats(float time, int savedScore, int kills)
    {
        playTime = time;
        score = savedScore;
        runKills = kills;
    }

    public void AddScore(int n)
    {
        score += n;
        if (score < 0) score = 0;
        StopCoroutine(nameof(ScorePop));
        StartCoroutine(ScorePop());
    }

    public void GameOver()
    {
        if (State != GameState.Playing) return;
        State = GameState.GameOver;
        Time.timeScale = 0;

        // 货币仅在死亡时统一结算（每杀 1 个敌人 = 1 货币）
        earnedCurrency = runKills;
        MetaStore.AddCurrency(earnedCurrency);
        // 槽位存档由玩家手动管理，死亡不删除任何存档

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }
        Debug.Log($"游戏结束！得分 {score}，最高 {highScore}，获得货币 {earnedCurrency}，按 R 重开 / M 回菜单");
    }

    public void Restart()
    {
        RunBootstrap.SlotToLoad = -1;   // R 重开 = 新的一局，不读档
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator ScorePop()
    {
        const float dur = 0.4f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);
            scoreScale = 1f + 0.35f * Mathf.Sin(k * Mathf.PI);
            yield return null;
        }
        scoreScale = 1f;
    }

    void OnGUI()
    {
        if (State == GameState.Playing)
        {
            float yTime = Screen.height - 64;
            float yScore = Screen.height - 34;
            GUI.Label(new Rect(10, yTime, 300, 30), "存活时间：" + playTime.ToString("F1") + " 秒");

            string label = "当前分数：  ";
            Vector2 tw = GUI.skin.label.CalcSize(new GUIContent(label));
            GUI.Label(new Rect(10, yScore, tw.x, 30), label);
            float nx = 10 + tw.x;
            GUI.matrix = Matrix4x4.TRS(new Vector3(nx, yScore, 0), Quaternion.identity,
                                       new Vector3(scoreScale, scoreScale, 1));
            GUI.Label(new Rect(0, 0, 160, 30), score.ToString());
            GUI.matrix = Matrix4x4.identity;
        }
        else if (State == GameState.GameOver)
        {
            GUI.Label(new Rect(10, Screen.height - 88, 400, 30),
                "存活时间：" + playTime.ToString("F1") + " 秒");
            GUI.Label(new Rect(10, Screen.height - 64, 400, 30),
                "得分：" + score + "   最高：" + highScore);
            GUI.Label(new Rect(10, Screen.height - 40, 400, 30),
                "本次获得货币：" + earnedCurrency);
            GUI.Label(new Rect(10, Screen.height - 16, 400, 30), "按 R 重开 / M 回主菜单");
        }
    }
}
