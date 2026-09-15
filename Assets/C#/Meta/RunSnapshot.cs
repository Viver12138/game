using UnityEngine;
using GameMeta;

/// <summary>
/// 单局快照采集：从场景中的玩家/管理器读取当前成长状态（仅手动保存时调用）
/// </summary>
public static class RunSnapshot
{
    public static RunSaveData Build(GameObject player, GameManager gm, PlayerExp exp, PlayerHealth hp)
    {
        var move = player.GetComponent<PlayerMove>();
        var holder = player.GetComponent<PlayerWeaponHolder>();
        Weapon weapon = holder != null ? holder.currentWeapon : null;

        var data = new RunSaveData
        {
            sceneName = "GameScene",
            stage = 1,

            playTime = gm.PlayTime,
            score = gm.Score,
            runKills = gm.RunKills,

            level = exp.level,
            exp = exp.exp,
            expToNext = exp.expToNext,
            currentHp = hp.hp,
            maxHp = hp.maxHp,
            energy = 0,   // 预留

            moveSpeed = move != null ? move.speed : 5f,
            weaponDamage = weapon != null ? weapon.damage : 0,

            vitPicks = exp.vitPicks,
            speedPicks = exp.speedPicks,
            damagePicks = exp.damagePicks,

            equippedWeaponName = weapon != null ? weapon.gameObject.name : "",
            equippedWeaponType = weapon != null ? (int)weapon.type : 0,

            playerX = player.transform.position.x,
            playerY = player.transform.position.y,
            playerZ = player.transform.position.z,
            playerRotY = player.transform.eulerAngles.y
        };
        return data;
    }
}
