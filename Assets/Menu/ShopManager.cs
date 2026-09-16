using UnityEngine;
using GameMeta;

/// <summary>
/// 主菜单"升级商店"（IMGUI）：消费永久货币购买三种永久强化。
/// 由 UIBootstrap 在 MainMenu 自动挂载。
/// </summary>
public class ShopManager : MonoBehaviour
{
    public bool IsOpen { get; private set; }

    const float PanelW = 520f;
    const float RowH = 96f, RowGap = 10f;

    public void Open()
    {
        MetaStore.Load();
        IsOpen = true;
    }

    public void Close() => IsOpen = false;

    void OnGUI()
    {
        if (!IsOpen) return;

        float panelH = 110 + (RowH + RowGap) * UpgradeCatalog.All.Count + 60;
        var area = new Rect((Screen.width - PanelW) / 2f, (Screen.height - panelH) / 2f, PanelW, panelH);

        GUI.Box(area, "");

        GUI.Label(new Rect(area.x + 20, area.y + 14, 200, 30), "升级商店",
            new GUIStyle(GUI.skin.label) { fontSize = 24 });
        GUI.Label(new Rect(area.xMax - 180, area.y + 18, 160, 26),
            "货币：" + MetaStore.Currency,
            new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleRight });

        float y = area.y + 60;
        foreach (var def in UpgradeCatalog.All)
        {
            DrawRow(new Rect(area.x + 16, y, PanelW - 32, RowH), def);
            y += RowH + RowGap;
        }

        if (GUI.Button(new Rect(area.center.x - 80, area.yMax - 48, 160, 34), "返回"))
            Close();
    }

    void DrawRow(Rect r, UpgradeCatalog.Def def)
    {
        GUI.Box(r, "");

        int level = MetaStore.GetLevel(def.id);
        int price = UpgradeCatalog.PriceAt(level);
        bool maxed = price < 0;
        bool canAfford = !maxed && MetaStore.Currency >= price;

        GUI.Label(new Rect(r.x + 14, r.y + 10, 260, 26),
            def.name + "   Lv." + level,
            new GUIStyle(GUI.skin.label) { fontSize = 18 });
        GUI.Label(new Rect(r.x + 14, r.y + 42, 260, 24),
            def.desc + " / 级",
            new GUIStyle(GUI.skin.label) { fontSize = 13 });
        GUI.Label(new Rect(r.x + 14, r.y + 66, 260, 22),
            maxed ? "已满级" : "下一级价格：" + price + " 货币",
            new GUIStyle(GUI.skin.label) { fontSize = 13 });

        var buyRect = new Rect(r.xMax - 130, r.y + (r.height - 40) / 2f, 116, 40);
        GUI.enabled = canAfford;
        string btnText = maxed ? "已满级" : "购买";
        if (GUI.Button(buyRect, btnText) && canAfford)
        {
            if (MetaStore.TrySpend(price))
                MetaStore.AddLevel(def.id);
        }
        GUI.enabled = true;
    }
}
