using System;
using System.Collections.Generic;

namespace GameMeta
{
    /// <summary>成就静态配置</summary>
    public static class AchievementCatalog
    {
        public class Def
        {
            public string id;
            public string name;
            public string desc;
            public int target;
        }

        static readonly List<Def> defs = new List<Def>
        {
            new Def { id = "kill_20",  name = "初级杀手", desc = "累计击杀20个敌人",  target = 20 },
            new Def { id = "kill_50",  name = "中级杀手", desc = "累计击杀50个敌人",  target = 50 },
            new Def { id = "kill_100", name = "高级杀手", desc = "累计击杀100个敌人", target = 100 },
        };

        public static IReadOnlyList<Def> All => defs;

        public static Def Get(string id) => defs.Find(d => d.id == id);
    }

    /// <summary>
    /// 成就解锁判定。解锁时写永久存档并广播事件（游戏内 Toast 订阅）。
    /// </summary>
    public static class AchievementService
    {
        public static event Action<AchievementCatalog.Def> OnUnlocked;

        /// <summary>根据累计击杀数扫描，解锁所有新达成的成就（历史玩家进入时补解锁也走这里）</summary>
        public static void CheckUnlocks(int totalKills)
        {
            bool changed = false;
            var saves = MetaStore.Data.achievements;

            foreach (var def in AchievementCatalog.All)
            {
                if (totalKills < def.target) continue;

                var save = saves.Find(a => a.id == def.id);
                if (save != null && save.unlocked) continue;

                if (save == null)
                {
                    save = new AchievementSaveData { id = def.id };
                    saves.Add(save);
                }
                save.unlocked = true;
                save.unlockedUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                changed = true;
                OnUnlocked?.Invoke(def);
            }

            if (changed) MetaStore.Save();
        }

        public static bool IsUnlocked(string id)
        {
            var save = MetaStore.Data.achievements.Find(a => a.id == id);
            return save != null && save.unlocked;
        }
    }
}
