using UnityEngine;

namespace Game
{
    public static class WeaponFactory
    {
        public static IWeapon CreateWeaponInstance(
            WeaponConfig config,
            IWeaponOwner owner,
            IWeaponEventListener[] listeners,
            int level)
        {
            if (config is RangedWeaponConfig rangedConfig)
            {
                return new RangedWeaponRuntime(rangedConfig, owner, listeners, level);
            }

            if (config is MeleeWeaponConfig meleeConfig)
            {
                return new MeleeWeaponRuntime(meleeConfig, owner, listeners, level);
            }

            if (config is ThrowableWeaponConfig throwableConfig)
            {
                return new ThrowableWeaponRuntime(throwableConfig, owner, listeners, level);
            }

            Debug.LogWarning($"WeaponFactory: unknown config type {config.GetType().Name}");
            return null;
        }
    }
}
