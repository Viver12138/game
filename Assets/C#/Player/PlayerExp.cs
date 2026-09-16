using System;
using UnityEngine;

public class PlayerExp : MonoBehaviour
{
    public int exp = 0;
    public int level = 1;
    public int expToNext = 5;    // 升级所需经验

    // 本局局内三选一强化的选择次数（读档重算用，由 UpgradeManager 回调累加）
    [System.NonSerialized] public int speedPicks;
    [System.NonSerialized] public int damagePicks;
    [System.NonSerialized] public int vitPicks;

    // ===== 新增：经验变化事件 =====
    // 参数1：当前经验  参数2：升级所需经验  参数3：当前等级
    public event Action<int, int, int> OnExpChanged;

    private PlayerMove playerMove; // 缓存组件，避免频繁 GetComponent

    void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
    }

    void Start()
    {
        // 开局广播一次，让 UI 初始化显示经验条
        OnExpChanged?.Invoke(exp, expToNext, level);
    }

    public void GainExp(int amount)
    {
        exp += amount;
        Debug.Log("获得经验 +" + amount + "，当前经验：" + exp + " / " + expToNext);

        bool leveled = false;
        while (exp >= expToNext)
        {
            exp -= expToNext;
            level++;
            expToNext += 3;
            leveled = true;
        }

        // ===== 新增：通知 UI 更新 =====
        OnExpChanged?.Invoke(exp, expToNext, level);
        if (leveled)
            UpgradeManager.Instance.RequestUpgrade();
    }

    /// <summary>读档恢复：按存档重设成长状态，并广播一次让 UI 同步</summary>
    public void RestoreRun(int savedLevel, int savedExp, int savedExpToNext,
                           int speedPicksCount, int damagePicksCount, int vitPicksCount)
    {
        level = savedLevel;
        exp = savedExp;
        expToNext = savedExpToNext;
        speedPicks = speedPicksCount;
        damagePicks = damagePicksCount;
        vitPicks = vitPicksCount;
        OnExpChanged?.Invoke(exp, expToNext, level);
    }
}