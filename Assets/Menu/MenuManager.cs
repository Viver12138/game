using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("目标游戏场景")]
    public string gameSceneName = "SampleScene";   // 改成你游戏场景的名字

    private int highScore;

    [Header("布局")]
    [Range(0f, 0.5f)] public float topOffset = 0.05f;

    void Start()
    {
        Time.timeScale = 1;                         // 保证菜单场景正常速度
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    void OnGUI()
    {
        float cw = Screen.width / 2f;
        float y = Screen.height * topOffset;      // ← 整个菜单从这开始往下排

        var title = new GUIStyle(GUI.skin.label) { fontSize = 48, alignment = TextAnchor.MiddleCenter };
        GUI.Label(new Rect(cw - 300, y, 600, 60), "霓虹幸存者", title);

        var sub = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
        GUI.Label(new Rect(cw - 200, y + 90, 400, 40), "历史最高分：" + highScore, sub);

        if (GUI.Button(new Rect(cw - 100, y + 160, 200, 50), "开始游戏"))
            SceneManager.LoadScene(gameSceneName);

        if (GUI.Button(new Rect(cw - 100, y + 230, 200, 50), "退出"))
            Application.Quit();
    }
}