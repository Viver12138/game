using System;
using UnityEngine;

public class PlayerExp : MonoBehaviour
{
    public int exp = 0;
    public int level = 1;
    public int expToNext = 5;    // 升级所需经验

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

        while (exp >= expToNext)      // 一次可能连升多级
        {
            exp -= expToNext;
            level++;
            expToNext += 3;

            // 使用缓存的 playerMove，避免每次升级都 GetComponent
            if (playerMove != null)
                playerMove.speed += 0.5f;

            Debug.Log("升级了！当前等级：" + level);
        }

        // ===== 新增：通知 UI 更新 =====
        // 放在 while 循环外面，无论是升级还是只加经验，都会触发
        OnExpChanged?.Invoke(exp, expToNext, level);
    }
}