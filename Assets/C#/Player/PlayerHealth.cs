using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("生命")]
    public int hp = 100;
    public int maxHp = 100;

    [Header("无敌帧")]
    public float invincibleTime = 1f;   // 受击后 1 秒内不再重复扣血

    // ===== 新增：血量变化事件 =====
    // 参数1：当前血量  参数2：最大血量
    public event Action<int, int> OnHealthChanged;

    private Animator animator;
    private float lastHitTime;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (hp > maxHp) hp = maxHp; // 防止 Inspector 里填错

        // 开局广播一次，让 UI 初始化显示满血
        OnHealthChanged?.Invoke(hp, maxHp);
    }

    /// <summary>
    /// 所有伤害入口：无敌帧 → 扣血 → 受击动画 → 死亡判断
    /// </summary>
    public void TakeDamage(int damage)
    {
        // 无敌帧：受击后一小段时间内不重复扣血
        if (Time.time - lastHitTime < invincibleTime) return;
        lastHitTime = Time.time;

        hp -= damage;
        if (hp < 0) hp = 0;                     // 防止变负数
        Debug.Log($"受伤！剩余血量：{hp}/{maxHp}");

        // ===== 新增：通知 UI 更新 =====
        OnHealthChanged?.Invoke(hp, maxHp);

        // 受击动画
        if (animator != null) animator.SetTrigger("IsHurt");

        // 死亡
        if (hp <= 0)
        {
            if (animator != null) animator.SetTrigger("IsDead");
            // 如果 GameManager 存在则调用，否则防报错
            if (GameManager.Instance != null) GameManager.Instance.GameOver();
        }
    }

    /// <summary>
    /// 治疗/补给（可选，捡血包用）
    /// </summary>
    public void Heal(int amount)
    {
        hp += amount;
        if (hp > maxHp) hp = maxHp;

        // ===== 新增：通知 UI 更新 =====
        OnHealthChanged?.Invoke(hp, maxHp);
    }

    /// <summary>
    /// 当前血量比例 0~1（备用）
    /// </summary>
    public float HpRatio => (float)hp / maxHp;
}