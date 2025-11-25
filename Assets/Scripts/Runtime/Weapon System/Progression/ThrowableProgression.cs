using System.Collections.Generic;
using System;
using UnityEngine;

namespace Game
{
    public class ThrowableProgression : ScriptableObject
    {
        public ThrowableStats baseStats;

        [Serializable]
        public class LevelData
        {
            public int level;
            public int requiredExp;
            public ThrowableLevelModifier modifier;
        }

        [SerializeField] private List<LevelData> levels = new List<LevelData>();

        public ThrowableStats GetStatsAtLevel(int level)
        {
            ThrowableStats stats = baseStats;

            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].level > level)
                    break;

                stats = levels[i].modifier.Apply(stats);
            }

            return stats;
        }
    }

    [Serializable]
    public struct ThrowableStats
    {
        public float throwForce;
        public float fuseTime;
        public float damage;
        public float radius;
        public bool damageFalloff;
    }

    [Serializable]
    public struct ThrowableLevelModifier
    {
        public float damageMul;
        public float radiusMul;
        public float throwForceMul;

        public ThrowableStats Apply(ThrowableStats baseStats)
        {
            baseStats.damage *= damageMul;
            baseStats.radius *= radiusMul;
            baseStats.throwForce *= throwForceMul;
            return baseStats;
        }
    }
}
