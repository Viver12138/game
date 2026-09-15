using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameMeta;

/// <summary>
/// 游戏场景开局引导：
/// 1) 新开局：应用商店购买的永久强化
/// 2) 读档：直接恢复存档内的最终属性快照（快照已含永久+局内强化）与成长/位置
/// 由 UIBootstrap 在 GameScene 自动挂载，无需手动拖引用。
/// </summary>
public class RunBootstrap : MonoBehaviour
{
    /// <summary>主菜单选择要加载的槽位；-1 = 新开局。进入 GameScene 后消费一次</summary>
    public static int SlotToLoad = -1;

    void Start()
    {
        StartCoroutine(WaitAFrame());
    }

    IEnumerator WaitAFrame()
    {
        // 等所有 Start 跑完：PlayerWeaponHolder 已 Equip 默认武器、HUD 已订阅事件
        yield return null;

        int slot = SlotToLoad;
        SlotToLoad = -1;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("RunBootstrap：未找到 Tag 为 Player 的对象");
            yield break;
        }

        var move = player.GetComponent<PlayerMove>();
        var health = player.GetComponent<PlayerHealth>();
        var exp = player.GetComponent<PlayerExp>();
        var holder = player.GetComponent<PlayerWeaponHolder>();

        RunSaveData run = null;
        if (slot >= 0)
        {
            var result = SaveManager.LoadSlot(slot);
            if (!result.success)
            {
                // 存档损坏等异常：明确提示并返回主菜单
                Debug.LogError("读档失败：" + result.error);
                Time.timeScale = 1;
                SceneManager.LoadScene("MainMenu");
                yield break;
            }
            run = result.data;
        }

        if (run != null)
        {
            // ===== 读档：以快照为准恢复最终状态 =====
            if (move != null) move.speed = run.moveSpeed;
            if (holder != null && holder.currentWeapon != null)
                holder.currentWeapon.damage = run.weaponDamage;

            health?.RestoreState(run.maxHp, run.currentHp);
            exp?.RestoreRun(run.level, run.exp, run.expToNext,
                            run.speedPicks, run.damagePicks, run.vitPicks);

            player.transform.position = new Vector3(run.playerX, run.playerY, run.playerZ);
            player.transform.rotation = Quaternion.Euler(0, run.playerRotY, 0);

            if (GameManager.Instance != null)
                GameManager.Instance.RestoreRunStats(run.playTime, run.score, run.runKills);
        }
        else
        {
            // ===== 新开局：基础值 + 永久强化（先清空再写入，避免重复叠加）=====
            float baseSpeed = move != null ? move.speed : 5f;
            int baseMaxHp = health != null ? health.maxHp : 100;
            int baseDamage = holder != null && holder.currentWeapon != null
                ? holder.currentWeapon.damage : 0;

            int hpLevel = MetaStore.GetLevel(UpgradeCatalog.Hp);
            int speedLevel = MetaStore.GetLevel(UpgradeCatalog.Speed);
            int damageLevel = MetaStore.GetLevel(UpgradeCatalog.Damage);

            int finalMaxHp = baseMaxHp + hpLevel * 10;
            float finalSpeed = baseSpeed + speedLevel * 0.3f;
            int finalDamage = baseDamage + damageLevel * 2;

            if (move != null) move.speed = finalSpeed;
            if (holder != null && holder.currentWeapon != null)
                holder.currentWeapon.damage = finalDamage;
            health?.RestoreState(finalMaxHp, finalMaxHp);
        }
    }
}
