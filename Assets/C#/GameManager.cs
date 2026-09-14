using UnityEngine;
using UnityEngine.SceneManagement;   
using System.Collections;

public enum GameState
{
    Playing,  // 游戏中
    GameOver  // 游戏结束（等待重开）
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState State { get; private set; } = GameState.Playing;

    float playTime;              // 存活时间（秒）
    public int score = 0;        // 当前分数
    float scoreTimer;            // 每满 1 秒 +1 分
    public event System.Action OnScoreGained;  
    //private float scoreBounceY;

    private float scoreScale = 1f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        if (State == GameState.Playing)
        {
            playTime += Time.deltaTime;

            // 每秒加一分（生存得分）
            scoreTimer += Time.deltaTime;
            if (scoreTimer >= 1f) { scoreTimer -= 1f; score++; }
        }
        else if (State == GameState.GameOver)
        {
            if (Input.GetKeyDown(KeyCode.R)) Restart();
        }
    }

    /// <summary>给分数加分（比如打死一只怪 +10）</summary>
    public void AddScore(int n)
    {
        score += n;
        if (score < 0) score = 0;
        OnScoreGained?.Invoke();

        // ===== 击杀就弹跳（StopCoroutine 合并同一帧多杀）=====
        StopCoroutine(nameof(ScorePop));
        StartCoroutine(ScorePop());
    }

    public void GameOver()
    {
        if (State != GameState.Playing) return;
        State = GameState.GameOver;
        Debug.Log($"游戏结束！存活时间 {playTime:F1} 秒，分数 {score}，按 R 重新开始");
        Time.timeScale = 0;
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnGUI()
    {
        float yTime = Screen.height - 64;
        float yScore = Screen.height - 34;

        if (State == GameState.Playing)
        {
            // 存活时间（左下，不动）
            GUI.Label(new Rect(10, yTime, 300, 30), "存活时间：" + playTime.ToString("F1") + " 秒");

            // ===== 当前分数 =====
            string label = "当前分数：  ";
            Vector2 tw = GUI.skin.label.CalcSize(new GUIContent(label));
            GUI.Label(new Rect(10, yScore, tw.x, 30), label);            // 文字固定

            // ===== 数字：只放大，留在原位 =====
            float nx = 10 + tw.x;
            float ny = yScore;                                          // 不再上移
            GUI.matrix = Matrix4x4.TRS(new Vector3(nx, ny, 0), Quaternion.identity,
                                       new Vector3(scoreScale, scoreScale, 1));
            GUI.Label(new Rect(0, 0, 160, 30), score.ToString());
            GUI.matrix = Matrix4x4.identity;   // 用完必须还原
        }
        else if (State == GameState.GameOver)
        {
            GUI.Label(new Rect(10, yTime, 400, 30), "存活时间：" + playTime.ToString("F1") +
                " 秒   |   分数：" + score);
            GUI.Label(new Rect(10, Screen.height - 30, 400, 30), "按 R 重新开始");
        }
    }
    IEnumerator ScorePop()
    {
        const float dur = 0.4f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            scoreScale = 1f + 0.35f * Mathf.Sin(k * Mathf.PI);   // 冲到 1.35 再回 1
            yield return null;
        }
        scoreScale = 1f;   // 结束恢复原大小
    }

}