using System.Collections.Generic;
using UnityEngine;

namespace GameMeta
{
    /// <summary>
    /// 永久元数据的内存缓存 + 业务操作。场景启动时 Load()，变更后 MarkDirty()/Save()。
    /// </summary>
    public static class MetaStore
    {
        static MetaSaveData data;
        static bool loaded;
        static bool dirty;
        static float dirtyTimer;

        public static MetaSaveData Data
        {
            get
            {
                if (!loaded) Load();
                return data;
            }
        }

        public static void Load()
        {
            data = SaveManager.LoadMeta();
            if (data.upgrades == null) data.upgrades = new List<UpgradeLevelData>();
            if (data.achievements == null) data.achievements = new List<AchievementSaveData>();
            loaded = true;
            dirty = false;
        }

        public static void Save()
        {
            if (!loaded) return;
            SaveManager.SaveMeta(data);
            dirty = false;
            dirtyTimer = 0f;
        }

        /// <summary>节流落盘：高频事件（击杀）调用后，由 Tick 统一写入</summary>
        public static void MarkDirty() => dirty = true;

        /// <summary>由场景中的引导脚本每帧调用，距上次标记超过间隔才真正写盘</summary>
        public static void Tick(float dt)
        {
            if (!dirty) return;
            dirtyTimer += dt;
            if (dirtyTimer >= 3f) Save();
        }

        // ===== 货币 =====
        public static int Currency => Data.currency;

        public static void AddCurrency(int amount)
        {
            if (amount <= 0) return;
            Data.currency += amount;
            Save();
        }

        public static bool TrySpend(int amount)
        {
            if (amount < 0 || Data.currency < amount) return false;
            Data.currency -= amount;
            Save();
            return true;
        }

        // ===== 永久强化等级 =====
        public static int GetLevel(string id)
        {
            var item = Data.upgrades.Find(u => u.id == id);
            return item != null ? item.level : 0;
        }

        public static void AddLevel(string id)
        {
            var item = Data.upgrades.Find(u => u.id == id);
            if (item == null)
            {
                item = new UpgradeLevelData { id = id };
                Data.upgrades.Add(item);
            }
            item.level++;
            Save();
        }

        // ===== 击杀统计 =====
        public static int TotalKills => Data.totalKills;

        /// <summary>击杀上报：累计击杀 +1，并检测成就解锁。返回最新累计值。</summary>
        public static int RecordKill()
        {
            Data.totalKills++;
            AchievementService.CheckUnlocks(Data.totalKills);
            MarkDirty();   // 成就解锁会立即 Save；否则走节流
            return Data.totalKills;
        }
    }
}
