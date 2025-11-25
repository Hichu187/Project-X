using UnityEngine;

namespace Game
{
    public interface IWeaponEventListener
    {
        void OnWeaponEvent(in WeaponEvent e);
    }
    public struct WeaponEvent
    {
        public WeaponEventType type;
        public IWeapon weapon;
        public IWeaponOwner owner;
        public Vector3 origin;
        public Vector3 direction;
        public Vector3 hitPoint;
        public IDamageable hitTarget;
    }
}
