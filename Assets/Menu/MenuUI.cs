using System;
using UnityEngine;

/// <summary>
/// IMGUI 通用控件：响应式尺寸、悬停高亮、禁用态按钮与悬停提示。
/// </summary>
public static class MenuUI
{
    /// <summary>按屏幕高度缩放的响应式字号</summary>
    public static int ScaledFont(int baseSize)
    {
        return Mathf.Clamp(Mathf.RoundToInt(baseSize * Screen.height / 1080f), 12, 48);
    }

    public static GUIStyle Label(int fontSize, TextAnchor anchor = TextAnchor.MiddleCenter)
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontSize = ScaledFont(fontSize),
            alignment = anchor,
            wordWrap = true
        };
    }

    /// <summary>带悬停变色反馈的按钮，返回是否点击</summary>
    public static bool Button(Rect rect, string text, int fontSize = 18)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        var prev = GUI.backgroundColor;
        GUI.backgroundColor = hover ? new Color(0.85f, 0.92f, 1f) : Color.white;

        var style = new GUIStyle(GUI.skin.button) { fontSize = ScaledFont(fontSize) };
        bool click = GUI.Button(rect, text, style);

        GUI.backgroundColor = prev;
        return click;
    }

    /// <summary>禁用按钮（灰显），hover 时显示提示文本</summary>
    public static void DisabledButton(Rect rect, string text, string tooltip, int fontSize = 18)
    {
        var prev = GUI.color;
        GUI.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        var style = new GUIStyle(GUI.skin.button) { fontSize = ScaledFont(fontSize) };
        GUI.Button(rect, text, style);
        GUI.color = prev;

        if (rect.Contains(Event.current.mousePosition))
        {
            var size = Label(13, TextAnchor.MiddleCenter).CalcSize(new GUIContent(tooltip));
            var tipRect = new Rect(rect.xMax + 10, rect.y, size.x + 20, size.y + 10);
            GUI.Box(tipRect, GUIContent.none);
            GUI.Label(tipRect, tooltip, Label(13));
        }
    }

    /// <summary>全屏半透明遮罩</summary>
    public static void Dim(float alpha = 0.6f)
    {
        GUI.color = new Color(0, 0, 0, alpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    /// <summary>时间戳（秒，UTC）→ 本地时间字符串，精确到分钟</summary>
    public static string FormatTime(long unixSeconds)
    {
        return DateTimeOffset.FromUnixTimeSeconds(unixSeconds).LocalDateTime
            .ToString("yyyy-MM-dd HH:mm");
    }

    /// <summary>字节数 → 友好大小</summary>
    public static string FormatSize(int bytes)
    {
        if (bytes < 1024) return bytes + " B";
        return (bytes / 1024f).ToString("F1") + " KB";
    }
}
