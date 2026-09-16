using UnityEngine;
using GameMeta;

/// <summary>
/// 主菜单"成就"列表面板（IMGUI）：名称/描述/进度，已解锁与未解锁区分显示。
/// 由 UIBootstrap 在 MainMenu 自动挂载。
/// </summary>
public class AchievementPanel : MonoBehaviour
{
    public bool IsOpen { get; private set; }

    const float PanelW = 540f;
    const float RowH = 78f, RowGap = 8f;

    public void Open()
    {
        MetaStore.Load();
        // 历史玩家进入时补解锁（累计击杀已超标但事件没在游戏内弹过）
        AchievementService.CheckUnlocks(MetaStore.TotalKills);
        IsOpen = true;
    }

    public void Close() => IsOpen = false;

    void OnGUI()
    {
        if (!IsOpen) return;

        float panelH = 90 + (RowH + RowGap) * AchievementCatalog.All.Count + 60;
        var area = new Rect((Screen.width - PanelW) / 2f, (Screen.height - panelH) / 2f, PanelW, panelH);

        GUI.Box(area, "");
        GUI.Label(new Rect(area.x + 20, area.y + 14, 300, 30), "成就",
            new GUIStyle(GUI.skin.label) { fontSize = 24 });
        GUI.Label(new Rect(area.xMax - 200, area.y + 18, 180, 26),
            "累计击杀：" + MetaStore.TotalKills,
            new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleRight });

        float y = area.y + 58;
        foreach (var def in AchievementCatalog.All)
        {
            DrawRow(new Rect(area.x + 16, y, PanelW - 32, RowH), def);
            y += RowH + RowGap;
        }

        if (GUI.Button(new Rect(area.center.x - 80, area.yMax - 48, 160, 34), "返回"))
            Close();
    }

    void DrawRow(Rect r, AchievementCatalog.Def def)
    {
        bool unlocked = AchievementService.IsUnlocked(def.id);
        int progress = Mathf.Min(MetaStore.TotalKills, def.target);

        // 未解锁灰底，已解锁正常底色
        GUI.backgroundColor = unlocked ? new Color(0.85f, 0.95f, 1f) : new Color(0.75f, 0.75f, 0.75f);
        GUI.Box(r, "");
        GUI.backgroundColor = Color.white;

        var nameStyle = new GUIStyle(GUI.skin.label) { fontSize = 18 };
        if (!unlocked) nameStyle.normal.textColor = new Color(0.3f, 0.3f, 0.3f);

        GUI.Label(new Rect(r.x + 14, r.y + 8, 360, 26),
            (unlocked ? "[已解锁] " : "[未解锁] ") + def.name, nameStyle);
        GUI.Label(new Rect(r.x + 14, r.y + 36, 360, 22), def.desc,
            new GUIStyle(GUI.skin.label) { fontSize = 13 });

        // 进度文字 + 简易进度条
        string progressText = progress + " / " + def.target;
        GUI.Label(new Rect(r.xMax - 130, r.y + 8, 116, 24), progressText,
            new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleRight });

        float barW = 200, barH = 12;
        var barBg = new Rect(r.xMax - barW - 14, r.yMax - 22, barW, barH);
        GUI.Box(barBg, "");
        float ratio = Mathf.Clamp01((float)progress / def.target);
        if (ratio > 0f)
        {
            GUI.color = unlocked ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.5f, 0.6f, 0.9f);
            GUI.DrawTexture(new Rect(barBg.x + 2, barBg.y + 2, (barW - 4) * ratio, barH - 4),
                            Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
