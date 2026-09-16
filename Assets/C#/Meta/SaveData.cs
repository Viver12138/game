using System;
using System.Collections.Generic;

namespace GameMeta
{
    /// <summary>
    /// 单局存档（多槽位，手动保存）。包含规范要求的全部核心字段。
    /// 注：本游戏当前为单场景生存玩法，stage/energy/items/questStates 为预留字段。
    /// </summary>
    [Serializable]
    public class RunSaveData
    {
        public int version = 1;

        // —— 游戏进度标识 ——
        public string sceneName = "GameScene";   // 关卡/场景
        public int stage = 1;                    // 章节/关卡序号（预留）

        // —— 玩家状态 ——
        public float playTime;
        public int score;
        public int runKills;

        public int level;
        public int exp;
        public int expToNext;
        public int currentHp;
        public int maxHp;
        public int energy;          // 能量值（预留，当前游戏无能量系统，恒为 0）

        // 最终属性快照（由"基础值+永久强化+局内强化"重算得出）
        public float moveSpeed;
        public int weaponDamage;

        // 局内三选一强化次数（用于重算校验）
        public int vitPicks;
        public int speedPicks;
        public int damagePicks;

        // —— 物品/装备 ——
        public string equippedWeaponName = "";   // 当前武器
        public int equippedWeaponType;           // 0=近战 1=远程
        public List<string> items = new List<string>();   // 物品持有（预留，当前为空）

        // —— 关键任务完成状态（预留，当前为空）——
        public List<string> questStates = new List<string>();

        // —— 位置 ——
        public float playerX, playerY, playerZ;
        public float playerRotY;

        // —— 槽位元信息 ——
        public int slotIndex = -1;
        public string slotName = "";
        public long saveUnixTime;    // 存档创建时间戳（秒，UTC）
        public int payloadBytes;     // 存档数据大小（字节，加密前）
    }

    /// <summary>
    /// 永久元数据：货币 / 累计击杀 / 永久强化等级 / 成就（不属于手动存档，全局保留）
    /// </summary>
    [Serializable]
    public class MetaSaveData
    {
        public int version = 1;
        public int currency;
        public int totalKills;
        public List<UpgradeLevelData> upgrades = new List<UpgradeLevelData>();
        public List<AchievementSaveData> achievements = new List<AchievementSaveData>();
    }

    [Serializable]
    public class UpgradeLevelData
    {
        public string id;
        public int level;
    }

    [Serializable]
    public class AchievementSaveData
    {
        public string id;
        public bool unlocked;
        public long unlockedUnixTime;
    }
}
