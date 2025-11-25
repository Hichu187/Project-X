using UnityEngine;

namespace Game
{
    public class ThrowableWeaponConfig : WeaponConfig
    {
        public ThrowableType throwableType;
        public ThrowableProgression progression;

        public GameObject projectilePrefab;
        public float arcHeight = 0.5f;
    }
}
