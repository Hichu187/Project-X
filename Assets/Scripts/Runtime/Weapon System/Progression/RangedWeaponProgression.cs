using System.Collections.Generic;
using System;
using UnityEngine;

namespace Game
{
    public class RangedWeaponProgression : ScriptableObject
    {
        public RangedWeaponStats baseStats;

        [Serializable]
        public class LevelData
        {
            public int level;
            public int requiredExp;
            public RangedWeaponLevelModifier modifier;
        }

        [SerializeField] private List<LevelData> levels = new List<LevelData>();

        public RangedWeaponStats GetStatsAtLevel(int level)
        {
            RangedWeaponStats stats = baseStats;

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
    public struct RangedWeaponStats
    {
        public float damage;
        public float maxRange;
        public int magazineSize;
        public float fireRate;

        public float baseSpread;
        public float spreadPerShot;
        public float maxSpread;
        public float spreadRecovery;
        public float spreadDistanceScale;

        public float recoilPerShot;
        public float maxRecoil;
        public float recoilRecovery;
    }

    [Serializable]
    public struct RangedWeaponLevelModifier
    {
        public float damageMul;
        public float fireRateMul;
        public float recoilMul;
        public float spreadMul;
        public int bonusMagazineSize;

        public RangedWeaponStats Apply(RangedWeaponStats baseStats)
        {
            baseStats.damage *= damageMul;
            baseStats.fireRate *= fireRateMul;

            baseStats.recoilPerShot *= recoilMul;
            baseStats.maxRecoil *= recoilMul;

            baseStats.baseSpread *= spreadMul;
            baseStats.spreadPerShot *= spreadMul;
            baseStats.maxSpread *= spreadMul;

            baseStats.magazineSize += bonusMagazineSize;

            return baseStats;
        }
    }
}
