using UnityEngine;

namespace Game
{
    public class ThrowableWeaponRuntime : IWeapon
    {
        public WeaponCategory Category => _config.category;
        public WeaponConfig Config => _config;
        public int Level => _level;

        private readonly ThrowableWeaponConfig _config;
        private readonly ThrowableProgression _progression;
        private readonly IWeaponOwner _owner;
        private readonly IWeaponEventListener[] _listeners;

        private ThrowableStats _stats;
        private int _level;
        private bool _fireHeldLastFrame;

        public ThrowableWeaponRuntime(
            ThrowableWeaponConfig config,
            IWeaponOwner owner,
            IWeaponEventListener[] listeners,
            int startLevel)
        {
            _config = config;
            _progression = config.progression;
            _owner = owner;
            _listeners = listeners;

            SetLevel(startLevel);
        }

        public void SetLevel(int level)
        {
            _level = level;
            _stats = _progression != null
                ? _progression.GetStatsAtLevel(level)
                : default;
        }

        public void Tick(float deltaTime)
        {
        }

        public void PrimaryFire(bool isHeld)
        {
            if (isHeld && !_fireHeldLastFrame)
            {
                Throw();
            }

            _fireHeldLastFrame = isHeld;
        }

        public void Reload()
        {
        }

        private void Throw()
        {
            if (_config.projectilePrefab == null) return;

            Vector3 origin = _owner.FirePoint.position;
            Vector3 dir = _owner.AimDirection.normalized;

            GameObject go = Object.Instantiate(
                _config.projectilePrefab,
                origin,
                Quaternion.LookRotation(dir));

            if (go.TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 velocity = dir;
                velocity.y += _config.arcHeight;
                velocity.Normalize();
                velocity *= _stats.throwForce;
                rb.linearVelocity = velocity;
            }

            var proj = go.GetComponent<ThrowableProjectile>();
            if (proj != null)
            {
                proj.Init(_config, _stats, _owner, _listeners, this);
            }

            RaiseEvent(
                WeaponEventType.ThrowableThrow,
                origin,
                dir,
                Vector3.zero,
                null);
        }

        internal void RaiseExplodeEvent(Vector3 origin)
        {
            RaiseEvent(
                WeaponEventType.ThrowableExplode,
                origin,
                Vector3.zero,
                origin,
                null);
        }

        private void RaiseEvent(
            WeaponEventType type,
            Vector3 origin,
            Vector3 dir,
            Vector3 hitPoint,
            IDamageable target)
        {
            if (_listeners == null) return;

            WeaponEvent e;
            e.type = type;
            e.weapon = this;
            e.owner = _owner;
            e.origin = origin;
            e.direction = dir;
            e.hitPoint = hitPoint;
            e.hitTarget = target;

            for (int i = 0; i < _listeners.Length; i++)
                _listeners[i].OnWeaponEvent(e);
        }
    }
}
