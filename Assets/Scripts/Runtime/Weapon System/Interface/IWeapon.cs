using UnityEngine;

namespace Game
{
    public interface IWeapon
    {
        WeaponCategory Category { get; }
        WeaponConfig Config { get; }
        int Level { get; }

        void Tick(float deltaTime);
        void PrimaryFire(bool isHeld);
        void Reload();
        void SetLevel(int level);
    }
}
