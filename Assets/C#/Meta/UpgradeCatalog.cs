using System;
using System.Collections.Generic;

namespace GameMeta
{
    /// <summary>
    /// 永久强化静态配置：效果/级、基础价、1.5 倍递增，20 级封顶
    /// </summary>
    public static class UpgradeCatalog
    {
        public const string Hp = "hp";         // 强化力量：最大生命
        public const string Speed = "speed";   // 强化速度：移动速度
        public const string Damage = "damage"; // 强化攻击：武器伤害

        public const int MaxLevel = 20;
        const int BasePrice = 10;

        public class Def
        {
            public string id;
            public string name;
            public string desc;      // 每级效果描述
            public float effectPerLevel;
        }

        static readonly List<Def> defs = new List<Def>
        {
            new Def { id = Hp,     name = "强化力量", desc = "最大生命 +10", effectPerLevel = 10f },
            new Def { id = Speed,  name = "强化速度", desc = "移动速度 +0.3", effectPerLevel = 0.3f },
            new Def { id = Damage, name = "强化攻击", desc = "武器伤害 +2", effectPerLevel = 2f },
        };

        public static IReadOnlyList<Def> All => defs;

        public static Def Get(string id) => defs.Find(d => d.id == id);

        /// <summary>当前等级购买下一级的价格：round(10 × 1.5^Lv)；满级返回 -1</summary>
        public static int PriceAt(int currentLevel)
        {
            if (currentLevel >= MaxLevel) return -1;
            return (int)Math.Round(BasePrice * Math.Pow(1.5f, currentLevel));
        }
    }
}
