namespace Game
{
    public enum WeaponCategory
    {
        Melee,
        Pistol,
        SMG,
        AR,
        Sniper,
        Rocket,
        Throwable,
    }

    public enum FireMode
    {
        Single,
        Burst,
        Auto,
    }

    public enum ThrowableType
    {
        Grenade,
        Flash,
        Molotov,
        Kunai,
    }

    public enum WeaponEventType
    {
        Fired,
        Hit,
        ReloadStart,
        ReloadEnd,
        EmptyTrigger,
        Equip,
        Unequip,
        MeleeSwing,
        MeleeHit,
        ThrowableThrow,
        ThrowableExplode,
    }

}
