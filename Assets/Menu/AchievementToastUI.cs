using System.Collections.Generic;
using UnityEngine;
using GameMeta;

/// <summary>
/// 成就解锁右下角弹窗（IMGUI）：队列依次播放，滑入 → 停留 → 滑出淡出。
/// 由 UIBootstrap 在 GameScene 自动挂载。
/// </summary>
public class AchievementToastUI : MonoBehaviour
{
    class Toast
    {
        public string title;
        public string desc;
        public float age;
        public const float Life = 3f;
        public const float Fade = 0.35f;
    }

    readonly Queue<Toast> queue = new Queue<Toast>();
    Toast current;

    void OnEnable() => AchievementService.OnUnlocked += HandleUnlocked;
    void OnDisable() => AchievementService.OnUnlocked -= HandleUnlocked;

    void HandleUnlocked(AchievementCatalog.Def def)
    {
        queue.Enqueue(new Toast { title = "成就解锁：" + def.name, desc = def.desc });
    }

    void Update()
    {
        if (current == null && queue.Count > 0)
            current = queue.Dequeue();
        if (current != null)
        {
            current.age += Time.unscaledDeltaTime;
            if (current.age >= Toast.Life) current = null;
        }
    }

    void OnGUI()
    {
        if (current == null) return;

        const float w = 280f, h = 74f;
        float margin = 20f;
        float baseX = Screen.width - w - margin;
        float baseY = Screen.height - h - margin;

        // 滑入/滑出进度 → 水平偏移和透明度
        float age = current.age;
        float offset, alpha;
        if (age < Toast.Fade)
        {
            float k = age / Toast.Fade;                       // 0→1 滑入
            offset = (1f - EaseOutCubic(k)) * (w + margin);
            alpha = k;
        }
        else if (age > Toast.Life - Toast.Fade)
        {
            float k = (age - (Toast.Life - Toast.Fade)) / Toast.Fade;  // 0→1 滑出
            offset = EaseInCubic(k) * (w + margin);
            alpha = 1f - k;
        }
        else
        {
            offset = 0f;
            alpha = 1f;
        }

        GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
        var rect = new Rect(baseX + offset, baseY, w, h);
        GUI.Box(rect, "");
        GUI.color = new Color(1f, 0.92f, 0.4f, Mathf.Clamp01(alpha));
        GUI.Label(new Rect(rect.x + 12, rect.y + 8, w - 24, 26), current.title,
            new GUIStyle(GUI.skin.label) { fontSize = 16 });
        GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
        GUI.Label(new Rect(rect.x + 12, rect.y + 38, w - 24, 26), current.desc,
            new GUIStyle(GUI.skin.label) { fontSize = 13 });
        GUI.color = Color.white;
    }

    static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    static float EaseInCubic(float t) => t * t * t;
}
