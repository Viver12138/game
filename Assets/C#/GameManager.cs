using UnityEngine;
using UnityEngine.SceneManagement;   // 重开场景必须引入

/// <summary>
/// 游戏状态（定义在类外面，方便所有脚本直接引用）
/// </summary>
public enum GameState
{
    Playing,  // 游戏中
    GameOver  // 游戏结束（等待重开）
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;   // 全局单例，其他脚本通过 GameManager.Instance 访问

    public GameState State { get; private set; } = GameState.Playing;

    float playTime;   // 本次存活时间

    void Awake()
    {
        // 单例保护：场景里只能有一个 GameManager
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
        }
        else if (State == GameState.GameOver)
        {
            // 游戏结束后按 R 重开
            if (Input.GetKeyDown(KeyCode.R))
            {
                Restart();
            }
        }
    }

    /// <summary>
    /// 玩家死亡时调用：进入游戏结束状态，冻结全场
    /// </summary>
    public void GameOver()
    {
        if (State != GameState.Playing) return;   // 防止重复触发
        State = GameState.GameOver;
        Debug.Log("游戏结束！存活时间：" + playTime.ToString("F1") + " 秒，按 R 重新开始");
        Time.timeScale = 0;   // 时间暂停 = 全游戏冻结
    }

    /// <summary>
    /// 重新开始：恢复时间并重载当前场景（所有物体和状态都会重置）
    /// </summary>
    public void Restart()
    {
        Time.timeScale = 1;   // 必须先恢复时间！否则新场景也会冻结
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnGUI()
    {
        if (State == GameState.Playing)
        {
            //GUI.Label(new Rect(10, 10, 300, 30), "存活时间：" + playTime.ToString("F1") + " 秒");
        }
        else if (State == GameState.GameOver)
        {
            GUI.Label(new Rect(10, 10, 300, 60),
                "游戏结束！\n存活时间：" + playTime.ToString("F1") + " 秒\n按 R 重新开始");
            // 注意：OnGUI 不受 Time.timeScale 影响，暂停后依然能显示
        }
    }
}