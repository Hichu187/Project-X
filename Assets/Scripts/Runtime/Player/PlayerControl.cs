using Sirenix.OdinInspector;
using UnityEngine;
using BaXoai;
using KinematicCharacterController;

namespace Game
{
    public class PlayerControl : MonoBehaviour
    {
        public CharacterControl characterControl;
        public VariableJoystick moveJoystick;
        public VariableJoystick lookJoystick;

        [OnValueChanged(nameof(OnControlModeChanged))]
        public bool isMobileController = false;

        private const string HorizontalInput = "Horizontal";
        private const string VerticalInput = "Vertical";

        private KinematicCharacterMotor motor;
        [SerializeField] private WeaponHolder weaponHolder;

        private void Start()
        {
            if (characterControl == null)
                characterControl = GetComponentInChildren<CharacterControl>(true);

            if (characterControl != null && motor == null)
                motor = characterControl.GetComponent<KinematicCharacterMotor>();

            if (weaponHolder == null)
                weaponHolder = GetComponentInChildren<WeaponHolder>(true);

            ApplyControlMode();
        }

        private void Update()
        {
            HandleMouseLookControl();
            HandleCharacterInput();
            HandleWeaponInput();
        }

        private void LateUpdate()
        {
            HandleCameraInput();
        }

        private void HandleCameraInput()
        {
        }

        private void OnControlModeChanged()
        {
            ApplyControlMode();
        }

        private void ApplyControlMode()
        {
            if (moveJoystick != null)
                moveJoystick.gameObject.SetActive(isMobileController);

            if (lookJoystick != null)
                lookJoystick.gameObject.SetActive(isMobileController);

            if (characterControl != null)
            {
                if (isMobileController)
                {
                    characterControl.AlwaysLookAtMouse = false;
                    characterControl.OrientationMethod = OrientationMethod.TowardsJoystick;
                }
                else
                {
                    characterControl.OrientationMethod = OrientationMethod.TowardsMouse;
                }
            }
        }

        private void HandleMouseLookControl()
        {
            if (isMobileController || characterControl == null)
                return;

            characterControl.AlwaysLookAtMouse = Input.GetMouseButton(0);
        }

        private void HandleCharacterInput()
        {
            if (characterControl == null)
                return;

            PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();

            if (!isMobileController)
            {
                characterInputs.MoveAxisForward = Input.GetAxisRaw(VerticalInput);
                characterInputs.MoveAxisRight = Input.GetAxisRaw(HorizontalInput);

                characterInputs.CrouchDown = Input.GetKeyDown(KeyCode.C);
                characterInputs.CrouchUp = Input.GetKeyUp(KeyCode.C);

                characterInputs.LookJoystick = Vector2.zero;
            }
            else
            {
                characterInputs.MoveAxisForward = moveJoystick != null ? moveJoystick.Vertical : 0f;
                characterInputs.MoveAxisRight = moveJoystick != null ? moveJoystick.Horizontal : 0f;

                characterInputs.LookJoystick = lookJoystick != null
                    ? new Vector2(lookJoystick.Horizontal, lookJoystick.Vertical)
                    : Vector2.zero;

                characterInputs.CrouchDown = false;
                characterInputs.CrouchUp = false;
            }

            Camera cam = (characterControl.ViewCamera != null)
                ? characterControl.ViewCamera
                : Camera.main;

            characterInputs.CameraRotation = cam != null ? cam.transform.rotation : Quaternion.identity;

            characterControl.SetInputs(ref characterInputs);
        }

        private void HandleWeaponInput()
        {
            if (weaponHolder == null)
                return;

            if (!isMobileController)
            {
                bool fireHeld = Input.GetMouseButton(0);
                weaponHolder.SetFireInput(fireHeld);

                if (Input.GetKeyDown(KeyCode.R))
                    weaponHolder.Reload();

                if (Input.GetKeyDown(KeyCode.Q))
                    weaponHolder.PreviousWeapon();

                if (Input.GetKeyDown(KeyCode.E))
                    weaponHolder.NextWeapon();
            }
        }

        [Button]
        public void ActiveLookMouse(bool active)
        {
            if (characterControl != null)
                characterControl.AlwaysLookAtMouse = active;
        }

        public void Mobile_FireDown()
        {
            if (!isMobileController || weaponHolder == null) return;
            weaponHolder.SetFireInput(true);
        }

        public void Mobile_FireUp()
        {
            if (!isMobileController || weaponHolder == null) return;
            weaponHolder.SetFireInput(false);
        }

        public void Mobile_Reload()
        {
            if (!isMobileController || weaponHolder == null) return;
            weaponHolder.Reload();
        }

        public void Mobile_NextWeapon()
        {
            if (!isMobileController || weaponHolder == null) return;
            weaponHolder.NextWeapon();
        }

        public void Mobile_PreviousWeapon()
        {
            if (!isMobileController || weaponHolder == null) return;
            weaponHolder.PreviousWeapon();
        }
    }
}
