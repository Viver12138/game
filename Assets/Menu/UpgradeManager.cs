using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("升级图标")]
    public Texture2D speedIcon;    // 移速 (wingfoot)
    public Texture2D damageIcon;   // 伤害 (armor-punch)
    public Texture2D hpIcon;       // 最大生命 (heart-plus)

    private bool panelVisible;
    private UpgradeOption[] options;
    private Transform player;

    class UpgradeOption
    {
        public string name;
        public string desc;
        public Texture2D icon;
        public System.Action onChoose;
    }

    void Awake() { Instance = this; }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    // ===== 外界调用：升级时弹面板 =====
    public void RequestUpgrade()
    {
        if (panelVisible) return;
        options = BuildRandomChoices();
        panelVisible = true;
        Time.timeScale = 0;       // 冻结世界，等玩家选
    }

    UpgradeOption[] BuildRandomChoices()
    {
        UpgradeOption[] all =
        {
            SpeedUp(), DamageUp(), HpUp()
        };
        // 打乱顺序（三种都显示，只是随机排列；以后类型多了再改成抽3个）
        for (int i = all.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (all[i], all[j]) = (all[j], all[i]);
        }
        return all;
    }

    UpgradeOption SpeedUp() => new UpgradeOption
    {
        name = "疾风",
        desc = "移动速度 +0.5",
        icon = speedIcon,
        onChoose = () => { if (player) player.GetComponent<PlayerMove>().speed += 0.5f; }
    };

    UpgradeOption DamageUp() => new UpgradeOption
    {
        name = "重击",
        desc = "当前武器伤害 +2",
        icon = damageIcon,
        onChoose = () =>
        {
            var holder = player ? player.GetComponent<PlayerWeaponHolder>() : null;
            if (holder != null && holder.currentWeapon != null)
                holder.currentWeapon.damage += 2;
        }
    };

    UpgradeOption HpUp() => new UpgradeOption
    {
        name = "体魄",
        desc = "最大生命值 +20",
        icon = hpIcon,
        onChoose = () =>
        {
            var h = player ? player.GetComponent<PlayerHealth>() : null;
            if (h != null) { h.maxHp += 20; h.hp += 20; }
        }
    };

    // ===== 面板绘制 =====
    void OnGUI()
    {
        if (!panelVisible || options == null) return;

        // 半透明遮罩
        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // 提示
        GUI.Label(new Rect(Screen.width / 2f - 100, Screen.height * 0.05f, 200, 40),
            "选择升级", new GUIStyle(GUI.skin.label) { fontSize = 30, alignment = TextAnchor.MiddleCenter });

        float cw = Screen.width / 2f;
        float cy = Screen.height * 0.25f;
        float cardW = 180, cardH = 250, gap = 20;
        float total = options.Length * cardW + (options.Length - 1) * gap;
        float x0 = cw - total / 2f;

        var titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
        var descStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, wordWrap = true, alignment = TextAnchor.MiddleCenter };

        for (int i = 0; i < options.Length; i++)
        {
            var o = options[i];
            Rect card = new Rect(x0 + i * (cardW + gap), cy, cardW, cardH);

            GUI.Box(card, "");
            if (o.icon != null)
                GUI.DrawTexture(new Rect(card.x + cardW / 2 - 32, card.y + 20, 64, 64), o.icon);
            GUI.Label(new Rect(card.x + 10, card.y + 95, cardW - 20, 30), o.name, titleStyle);
            GUI.Label(new Rect(card.x + 10, card.y + 135, cardW - 20, 70), o.desc, descStyle);

            if (GUI.Button(new Rect(card.x + 20, card.y + cardH - 50, cardW - 40, 34), "选择"))
            {
                o.onChoose();
                panelVisible = false;
                Time.timeScale = 1;      // 恢复游戏
            }
        }
    }
}