using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("数据源")]
    public PlayerHealth playerHealth;
    public PlayerExp playerExp;

    [Header("UI 引用")]
    public Slider healthSlider;
    public Slider expSlider;

    void Start()
    {
        // ===== 血条初始化 =====
        if (playerHealth != null && healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = playerHealth.maxHp; // 对应 PlayerHealth 的 maxHp
            healthSlider.value = playerHealth.hp;       // 对应 PlayerHealth 的 hp
            playerHealth.OnHealthChanged += UpdateHealth;
        }

        // ===== 经验条初始化 =====
        if (playerExp != null && expSlider != null)
        {
            expSlider.minValue = 0;
            expSlider.maxValue = playerExp.expToNext; // 对应 PlayerExp 的 expToNext
            expSlider.value = playerExp.exp;          // 对应 PlayerExp 的 exp（已修正）
            playerExp.OnExpChanged += UpdateExp;
        }
    }

    void OnDestroy()
    {
        // 取消订阅，防止内存泄漏
        if (playerHealth != null) playerHealth.OnHealthChanged -= UpdateHealth;
        if (playerExp != null) playerExp.OnExpChanged -= UpdateExp;
    }

    void UpdateHealth(int current, int max)
    {
        healthSlider.maxValue = max;
        healthSlider.value = current;
    }

    void UpdateExp(int current, int needed, int level)
    {
        expSlider.maxValue = needed;
        expSlider.value = current;
        // 如果以后想显示等级文本，可以在这里加：levelText.text = "Lv." + level;
    }
}