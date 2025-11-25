using UnityEngine;

namespace Game
{
    public class ThrowableProjectile : MonoBehaviour
    {
        private ThrowableWeaponConfig _config;
        private ThrowableStats _stats;
        private IWeaponOwner _owner;
        private IWeaponEventListener[] _listeners;
        private ThrowableWeaponRuntime _runtime;

        private float _timer;

        public void Init(
            ThrowableWeaponConfig config,
            ThrowableStats stats,
            IWeaponOwner owner,
            IWeaponEventListener[] listeners,
            ThrowableWeaponRuntime runtime)
        {
            _config = config;
            _stats = stats;
            _owner = owner;
            _listeners = listeners;
            _runtime = runtime;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _stats.fuseTime)
            {
                Explode();
            }
        }

        private void Explode()
        {
            Vector3 center = transform.position;

            Collider[] hits = Physics.OverlapSphere(center, _stats.radius);
            for (int i = 0; i < hits.Length; i++)
            {
                IDamageable dmg = hits[i].GetComponentInParent<IDamageable>();
                if (dmg == null) continue;

                float finalDamage = _stats.damage;

                if (_stats.damageFalloff)
                {
                    float dist = Vector3.Distance(center, hits[i].transform.position);
                    float t = Mathf.Clamp01(dist / _stats.radius);
                    finalDamage = Mathf.Lerp(_stats.damage, 0f, t);
                }

                dmg.TakeDamage(finalDamage, center, Vector3.up);
            }

            if (_runtime != null)
                _runtime.RaiseExplodeEvent(center);

            RaiseLocalExplodeEvent(center);

            Destroy(gameObject);
        }

        private void RaiseLocalExplodeEvent(Vector3 origin)
        {
            if (_listeners == null) return;

            WeaponEvent e;
            e.type = WeaponEventType.ThrowableExplode;
            e.weapon = _runtime;
            e.owner = _owner;
            e.origin = origin;
            e.direction = Vector3.zero;
            e.hitPoint = origin;
            e.hitTarget = null;

            for (int i = 0; i < _listeners.Length; i++)
                _listeners[i].OnWeaponEvent(e);
        }
    }
}
