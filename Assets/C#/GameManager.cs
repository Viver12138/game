using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public enum GameState
{
    Playing,   // 游戏中
    GameOver   // 游戏结束
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState State { get; private set; } = GameState.Playing;

    float playTime;
    public int score = 0;
    float scoreTimer;
    int highScore;
    float scoreScale = 1f;
    const string HighScoreKey = "HighScore";

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
    }

    void Update()
    {
        if (State == GameState.Playing)
        {
            playTime += Time.deltaTime;
            scoreTimer += Time.deltaTime;
            if (scoreTimer >= 1f) { scoreTimer -= 1f; score++; }
        }
        else if (State == GameState.GameOver)
        {
            if (Input.GetKeyDown(KeyCode.R)) Restart();
            if (Input.GetKeyDown(KeyCode.M)) SceneManager.LoadScene("MainMenu");  // 或回菜单：见下
        }
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
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }
        Debug.Log($"游戏结束！得分 {score}，最高 {highScore}，按 R 重开 / M 回菜单");
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
            GUI.Label(new Rect(10, Screen.height - 64, 400, 30), "得分：" + score + "   最高：" + highScore);
            GUI.Label(new Rect(10, Screen.height - 30, 400, 30), "按 R 重开 / M 回主菜单");
        }
    }
}