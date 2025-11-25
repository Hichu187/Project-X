using System.Collections.Generic;
using System;
using UnityEngine;

namespace Game
{
    public class MeleeWeaponProgression : ScriptableObject
    {
        public MeleeWeaponStats baseStats;

        [Serializable]
        public class LevelData
        {
            public int level;
            public int requiredExp;
            public MeleeWeaponLevelModifier modifier;
        }

        [SerializeField] private List<LevelData> levels = new List<LevelData>();

        public MeleeWeaponStats GetStatsAtLevel(int level)
        {
            MeleeWeaponStats stats = baseStats;

            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].level > level)
                    break;

                stats = levels[i].modifier.Apply(stats);
            }

            return stats;
        }

        public int GetRequiredExpForLevel(int level)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].level == level)
                    return levels[i].requiredExp;
            }
            return 0;
        }
    }
    [Serializable]
    public struct MeleeWeaponStats
    {
        public float damage;
        public float range;
        public float attackRate;
        public float hitAngle;
    }

    [Serializable]
    public struct MeleeWeaponLevelModifier
    {
        public float damageMul;
        public float rangeMul;
        public float attackRateMul;

        public MeleeWeaponStats Apply(MeleeWeaponStats baseStats)
        {
            baseStats.damage *= damageMul;
            baseStats.range *= rangeMul;
            baseStats.attackRate *= attackRateMul;
            return baseStats;
        }
    }
}
