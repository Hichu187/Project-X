using UnityEngine;

namespace Game
{
    public interface IWeaponOwner
    {
        Transform FirePoint { get; }
        Vector3 AimDirection { get; }
        Transform RootTransform { get; }
    }
}
