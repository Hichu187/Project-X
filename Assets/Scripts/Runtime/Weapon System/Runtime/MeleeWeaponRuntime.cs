using System.Collections;
using UnityEngine;

namespace Game
{
    public class MeleeWeaponRuntime : IWeapon
    {
        public WeaponCategory Category => _config.category;
        public WeaponConfig Config => _config;
        public int Level => _level;

        private readonly MeleeWeaponConfig _config;
        private readonly MeleeWeaponProgression _progression;
        private readonly IWeaponOwner _owner;
        private readonly IWeaponEventListener[] _listeners;

        private MeleeWeaponStats _stats;
        private int _level;
        private float _lastAttackTime;
        private bool _isAttacking;

        public MeleeWeaponRuntime(
            MeleeWeaponConfig config,
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

            _lastAttackTime = -999f;
            _isAttacking = false;
        }

        public void Tick(float deltaTime)
        {
        }

        public void PrimaryFire(bool isHeld)
        {
            if (!isHeld) return;
            if (_isAttacking) return;

            float interval = 1f / Mathf.Max(0.01f, _stats.attackRate);
            if (Time.time - _lastAttackTime < interval)
                return;

            _lastAttackTime = Time.time;
            _isAttacking = true;

            RaiseEvent(
                WeaponEventType.MeleeSwing,
                _owner.FirePoint.position,
                _owner.AimDirection,
                Vector3.zero,
                null);

            WeaponCoroutineRunner.Run(AttackRoutine());
        }

        public void Reload()
        {
        }

        private IEnumerator AttackRoutine()
        {
            if (_config.attackWindup > 0f)
                yield return new WaitForSeconds(_config.attackWindup);

            PerformHit();
            _isAttacking = false;
        }

        private void PerformHit()
        {
            Vector3 origin = _owner.RootTransform.position;
            Vector3 forward = _owner.AimDirection.normalized;
            float radius = _stats.range;

            Collider[] hits = Physics.OverlapSphere(origin, radius);
            for (int i = 0; i < hits.Length; i++)
            {
                Transform t = hits[i].transform;
                Vector3 toTarget = t.position - origin;
                toTarget.y = 0f;

                float distance = toTarget.magnitude;
                if (distance > _stats.range)
                    continue;

                float angle = Vector3.Angle(forward, toTarget);
                if (angle > _stats.hitAngle * 0.5f)
                    continue;

                IDamageable dmg = t.GetComponentInParent<IDamageable>();
                if (dmg == null) continue;

                Vector3 hitPoint = t.position;
                dmg.TakeDamage(_stats.damage, hitPoint, -forward);

                RaiseEvent(
                    WeaponEventType.MeleeHit,
                    origin,
                    forward,
                    hitPoint,
                    dmg);
            }
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
