using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("数据源")]
    public PlayerHealth playerHealth;
    public PlayerExp playerExp;

    [Header("UI 组件")]
    public Slider healthSlider;   // 血条
    public Slider expSlider;      // 经验条
    public TMP_Text levelText;

    private int lastLevel;                  // 上一次等级（判断是否升级）
    private Vector3 baseScale = Vector3.one;
    private Color baseColor = Color.white;

    private Vector2 basePos;

    void Start()
    {
        // ===== 血条 =====
        if (playerHealth != null && healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = playerHealth.maxHp;
            healthSlider.value = playerHealth.hp;
            playerHealth.OnHealthChanged += UpdateHealth;
        }

        // ===== 经验条 =====
        if (playerExp != null && expSlider != null)
        {
            expSlider.minValue = 0;
            expSlider.maxValue = playerExp.expToNext;
            expSlider.value = playerExp.exp;
            playerExp.OnExpChanged += UpdateExp;
        }

        // ===== 等级文本初始 =====
        if (playerExp != null)
        {
            lastLevel = playerExp.level;
            levelText.text = "Lv." + playerExp.level;
        }
        if (levelText != null)
        {
            baseScale = levelText.rectTransform.localScale;
            baseColor = levelText.color;
            basePos = levelText.rectTransform.anchoredPosition;
        }


    }

    void OnDestroy()
    {
        if (playerHealth != null) playerHealth.OnHealthChanged -= UpdateHealth;
        if (playerExp != null) playerExp.OnExpChanged -= UpdateExp;
    }

    void UpdateHealth(int current, int max)
    {
        if (healthSlider == null) return;
        healthSlider.maxValue = max;
        healthSlider.value = current;
    }

    void UpdateExp(int current, int needed, int level)
    {
        if (expSlider != null)
        {
            expSlider.maxValue = needed;
            expSlider.value = current;
        }

        if (levelText != null)
        {
            levelText.text = "Lv." + level;

            // ===== 进阶：升了一级就弹一下+闪一下 =====
            if (level > lastLevel)
            {
                lastLevel = level;
                StopCoroutine(nameof(LevelUpPop));   // 连续升级也只会播最后一次
                StartCoroutine(LevelUpPop());
            }
        }
    }

    /// <summary>等级文本 punch：放大冲过再回落 + 短暂闪色</summary>
    IEnumerator LevelUpPop()
    {
        float upVel = 100f;          // 初始向上速度（像素/秒）——弹多高
        const float gravity = 1600f; // 重力（像素/秒²）——回落多快

        float y = 0f;
        while (y > 0f || upVel > 0f)      // 还没落地就继续
        {
            upVel -= gravity * Time.deltaTime;     // 重力减速
            y += upVel * Time.deltaTime;
            levelText.rectTransform.anchoredPosition = basePos + Vector2.up * y;
            yield return null;
        }
        levelText.rectTransform.anchoredPosition = basePos;   // 归位
    }
}