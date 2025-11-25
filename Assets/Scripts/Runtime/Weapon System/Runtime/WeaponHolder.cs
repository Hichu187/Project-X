using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class WeaponSlot
    {
        public WeaponConfig config;
        public GameObject weaponObject;
        public Transform firePointOverride;
        public int startLevel = 1;
    }

    public class WeaponHolder : MonoBehaviour, IWeaponOwner
    {
        [Header("References")]
        [SerializeField] private CharacterControl characterControl;

        [Header("Weapons")]
        [SerializeField] private List<WeaponSlot> weaponSlots = new List<WeaponSlot>();

        private IWeapon _currentWeapon;
        private int _currentIndex;
        private IWeaponEventListener[] _listeners;
        private bool _fireHeld;
        private Transform _currentFirePoint;

        public Transform FirePoint => _currentFirePoint != null ? _currentFirePoint : transform;

        public Vector3 AimDirection
        {
            get
            {
                if (characterControl != null && characterControl.Motor != null)
                {
                    Vector3 f = characterControl.Motor.CharacterForward;
                    f.y = 0f;
                    if (f.sqrMagnitude > 0.0001f)
                        return f.normalized;
                }

                Vector3 dir = transform.forward;
                dir.y = 0f;
                return dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.forward;
            }
        }

        public Transform RootTransform => transform;

        private void Awake()
        {
            if (characterControl == null)
                characterControl = GetComponent<CharacterControl>();

            _listeners = GetComponentsInChildren<IWeaponEventListener>(includeInactive: true);

            for (int i = 0; i < weaponSlots.Count; i++)
            {
                SetupSlotFirePoint(weaponSlots[i]);
                if (weaponSlots[i].weaponObject != null)
                    weaponSlots[i].weaponObject.SetActive(false);
            }

            EquipIndex(0);
        }

        private void Update()
        {
            if (_currentWeapon == null)
            {
                // Log 1 lần khi không có weapon để khỏi spam
                // Debug.LogWarning("[WeaponHolder] No current weapon equipped");
                return;
            }

            _currentWeapon.Tick(Time.deltaTime);

            // LOG: kiểm tra xem frame này có đang yêu cầu bắn không
            if (_fireHeld)
            {
                Debug.Log($"[WeaponHolder] FireHeld = TRUE at time {Time.time}");
            }

            _currentWeapon.PrimaryFire(_fireHeld);
        }

        void SetupSlotFirePoint(WeaponSlot slot)
        {
            if (slot.weaponObject == null)
                return;

            if (slot.firePointOverride != null)
                return;

            Transform fp = slot.weaponObject.transform.Find("FirePoint");
            if (fp == null)
                fp = slot.weaponObject.transform.Find("Muzzle");

            slot.firePointOverride = fp;
        }

        public void SetFireInput(bool isHeld)
        {
            _fireHeld = isHeld;
        }

        public void Reload()
        {
            if (_currentWeapon == null) return;
            _currentWeapon.Reload();
        }

        public void NextWeapon()
        {
            if (weaponSlots.Count == 0) return;
            int newIndex = (_currentIndex + 1) % weaponSlots.Count;
            EquipIndex(newIndex);
        }

        public void PreviousWeapon()
        {
            if (weaponSlots.Count == 0) return;
            int newIndex = _currentIndex - 1;
            if (newIndex < 0)
                newIndex = weaponSlots.Count - 1;
            EquipIndex(newIndex);
        }

        public void EquipIndex(int index)
        {
            if (weaponSlots.Count == 0)
            {
                _currentWeapon = null;
                _currentFirePoint = null;
                return;
            }

            index = Mathf.Clamp(index, 0, weaponSlots.Count - 1);
            _currentIndex = index;

            for (int i = 0; i < weaponSlots.Count; i++)
            {
                if (weaponSlots[i].weaponObject != null)
                    weaponSlots[i].weaponObject.SetActive(i == _currentIndex);
            }

            WeaponSlot slot = weaponSlots[_currentIndex];

            SetupSlotFirePoint(slot);
            _currentFirePoint = slot.firePointOverride != null
                ? slot.firePointOverride
                : slot.weaponObject != null ? slot.weaponObject.transform : transform;

            if (slot.config != null)
            {
                _currentWeapon = WeaponFactory.CreateWeaponInstance(
                    slot.config,
                    this,
                    _listeners,
                    slot.startLevel);
            }
            else
            {
                _currentWeapon = null;
            }

            RaiseEquipEvent();
        }

        void RaiseEquipEvent()
        {
            if (_currentWeapon == null || _listeners == null)
                return;

            WeaponEvent e;
            e.type = WeaponEventType.Equip;
            e.weapon = _currentWeapon;
            e.owner = this;
            e.origin = FirePoint.position;
            e.direction = AimDirection;
            e.hitPoint = Vector3.zero;
            e.hitTarget = null;

            for (int i = 0; i < _listeners.Length; i++)
                _listeners[i].OnWeaponEvent(e);
        }
    }
}
