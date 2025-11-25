using UnityEngine;

namespace Game
{
    public class RangedWeaponRuntime : IWeapon
    {
        public WeaponCategory Category => _config.category;
        public WeaponConfig Config => _config;
        public int Level => _level;

        private readonly RangedWeaponConfig _config;
        private readonly RangedWeaponProgression _progression;
        private readonly IWeaponOwner _owner;
        private readonly IWeaponEventListener[] _listeners;

        private RangedWeaponStats _stats;
        private int _level;
        private int _currentAmmo;
        private float _lastFireTime;
        private float _currentSpread;
        private float _currentRecoil;

        public RangedWeaponRuntime(
            RangedWeaponConfig config,
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
            _stats = _progression != null ? _progression.GetStatsAtLevel(level) : default;

            _currentAmmo = _stats.magazineSize;
            _currentSpread = 0f;
            _currentRecoil = 0f;
            _lastFireTime = -999f;
        }

        public void Tick(float deltaTime)
        {
            _currentSpread = Mathf.MoveTowards(_currentSpread, 0f, _stats.spreadRecovery * deltaTime);
            _currentRecoil = Mathf.MoveTowards(_currentRecoil, 0f, _stats.recoilRecovery * deltaTime);
        }

        public void PrimaryFire(bool isHeld)
        {
            if (!isHeld)
                return;

            float interval = 1f / Mathf.Max(0.01f, _stats.fireRate);
            if (Time.time - _lastFireTime < interval)
                return;

            if (_currentAmmo <= 0)
            {
                RaiseEvent(WeaponEventType.EmptyTrigger);
                return;
            }

            _lastFireTime = Time.time;
            _currentAmmo--;

            Vector3 origin = _owner.FirePoint.position;
            Vector3 baseDir = _owner.AimDirection.normalized;
            float distance = _stats.maxRange;

            Vector3 shotDir = ApplySpread(baseDir, distance);

            if (_config.useRaycast)
                DoRaycast(origin, shotDir);
            else
                SpawnProjectile(origin, shotDir);

            _currentSpread = Mathf.Min(_currentSpread + _stats.spreadPerShot, _stats.maxSpread);
            _currentRecoil = Mathf.Min(_currentRecoil + _stats.recoilPerShot, _stats.maxRecoil);

            RaiseEvent(WeaponEventType.Fired, origin, shotDir, Vector3.zero, null);
        }

        public void Reload()
        {
            RaiseEvent(WeaponEventType.ReloadStart);
            _currentAmmo = _stats.magazineSize;
            RaiseEvent(WeaponEventType.ReloadEnd);
        }

        private Vector3 ApplySpread(Vector3 forward, float distance)
        {
            Vector3 flatForward = forward;
            flatForward.y = 0f;
            if (flatForward.sqrMagnitude < 0.0001f)
                flatForward = Vector3.forward;
            flatForward.Normalize();

            float distScale = _stats.spreadDistanceScale <= 0f ? 1f : (1f + distance / _stats.spreadDistanceScale);
            float spreadAngle = _currentSpread * distScale;

            float yaw = Random.Range(-spreadAngle, spreadAngle);
            Quaternion rot = Quaternion.AngleAxis(yaw, Vector3.up);

            Vector3 dir = rot * flatForward;
            dir.y = 0f;
            return dir.normalized;
        }

        private void DoRaycast(Vector3 origin, Vector3 dir)
        {
            float maxDist = _stats.maxRange;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDist))
            {
                TracerSystem.SpawnTracer(origin, hit.point, Color.yellow, 0.04f);

                IDamageable dmg = hit.collider.GetComponentInParent<IDamageable>();
                if (dmg != null)
                {
                    dmg.TakeDamage(_stats.damage, hit.point, hit.normal);
                    RaiseEvent(WeaponEventType.Hit, origin, dir, hit.point, dmg);
                }
            }
            else
            {
                Vector3 end = origin + dir * maxDist;
                TracerSystem.SpawnTracer(origin, end, Color.yellow, 0.04f);
            }
        }

        private void SpawnProjectile(Vector3 origin, Vector3 dir)
        {
            if (_config.projectilePrefab == null)
                return;

            GameObject go = Object.Instantiate(
                _config.projectilePrefab,
                origin,
                Quaternion.LookRotation(dir));

            if (go.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = dir * _config.projectileSpeed;
        }

        private void RaiseEvent(
            WeaponEventType type,
            Vector3 origin = default,
            Vector3 direction = default,
            Vector3 hitPoint = default,
            IDamageable target = null)
        {
            if (_listeners == null) return;

            WeaponEvent e;
            e.type = type;
            e.weapon = this;
            e.owner = _owner;
            e.origin = origin;
            e.direction = direction;
            e.hitPoint = hitPoint;
            e.hitTarget = target;

            for (int i = 0; i < _listeners.Length; i++)
                _listeners[i].OnWeaponEvent(e);
        }

        public int CurrentAmmo => _currentAmmo;
        public int MagazineSize => _stats.magazineSize;
    }
}
