using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using BaXoai;
using KinematicCharacterController;

namespace Game
{
    public class PlayerControl : MonoBehaviour
    {
        public CharacterControl characterControl;
        public VariableJoystick moveJoystick;
        public VariableJoystick lookJoystick;

        public bool isMobileController = false;

        private const string MouseXInput = "Mouse X";
        private const string MouseYInput = "Mouse Y";
        private const string MouseScrollInput = "Mouse ScrollWheel";
        private const string HorizontalInput = "Horizontal";
        private const string VerticalInput = "Vertical";

        private KinematicCharacterMotor motor;
        private void Start()
        {
            //Cursor.lockState = CursorLockMode.Locked;

            /*CharacterCamera.SetFollowTransform(Character.CameraFollowPoint);
            CharacterCamera.IgnoredColliders.Clear();
            CharacterCamera.IgnoredColliders.AddRange(Character.GetComponentsInChildren<Collider>());*/

            if (!motor) motor = characterControl.GetComponent<KinematicCharacterMotor>();
        }

        private void Update()
        {
            HandleCharacterInput();
        }

        private void LateUpdate()
        {
            // Handle rotating the camera along with physics movers
            /*if (CharacterCamera.RotateWithPhysicsMover && Character.Motor.AttachedRigidbody != null)
            {
                CharacterCamera.PlanarDirection = Character.Motor.AttachedRigidbody.GetComponent<PhysicsMover>().RotationDeltaFromInterpolation * CharacterCamera.PlanarDirection;
                CharacterCamera.PlanarDirection = Vector3.ProjectOnPlane(CharacterCamera.PlanarDirection, Character.Motor.CharacterUp).normalized;
            }*/

            HandleCameraInput();
        }

        private void HandleCameraInput()
        {
            /*// Create the look input vector for the camera
            float mouseLookAxisUp = Input.GetAxisRaw(MouseYInput);
            float mouseLookAxisRight = Input.GetAxisRaw(MouseXInput);
            Vector3 lookInputVector = new Vector3(mouseLookAxisRight, mouseLookAxisUp, 0f);

            // Prevent moving the camera while the cursor isn't locked
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                lookInputVector = Vector3.zero;
            }

            // Input for zooming the camera (disabled in WebGL because it can cause problems)
            float scrollInput = -Input.GetAxis(MouseScrollInput);
#if UNITY_WEBGL
            scrollInput = 0f;
#endif

            // Apply inputs to the camera
            CharacterCamera.UpdateWithInput(Time.deltaTime, scrollInput, lookInputVector);

            // Handle toggling zoom level
            if (Input.GetMouseButtonDown(1))
            {
                CharacterCamera.TargetDistance = (CharacterCamera.TargetDistance == 0f) ? CharacterCamera.DefaultDistance : 0f;
            }*/
        }

        private void HandleCharacterInput()
        {
            PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();

            if (!isMobileController)
            {
                // --- PC / Keyboard ---
                characterInputs.MoveAxisForward = Input.GetAxisRaw(VerticalInput);
                characterInputs.MoveAxisRight = Input.GetAxisRaw(HorizontalInput);

                characterInputs.JumpDown = Input.GetKeyDown(KeyCode.Space);
                characterInputs.CrouchDown = Input.GetKeyDown(KeyCode.C);
                characterInputs.CrouchUp = Input.GetKeyUp(KeyCode.C);

                characterInputs.LookJoystick = Vector2.zero;
            }
            else
            {
                // --- Mobile / Joystick ---
                if (moveJoystick != null)
                {
                    // Joystick: X = ngang, Y = dọc
                    characterInputs.MoveAxisForward = moveJoystick.Vertical;
                    characterInputs.MoveAxisRight = moveJoystick.Horizontal;
                }
                else
                {
                    characterInputs.MoveAxisForward = 0f;
                    characterInputs.MoveAxisRight = 0f;
                }

                if (lookJoystick != null)
                {
                    characterInputs.LookJoystick = new Vector2(lookJoystick.Horizontal, lookJoystick.Vertical);
                    // hoặc: characterInputs.LookJoystick = lookJoystick.Direction;
                }
                else
                {
                    characterInputs.LookJoystick = Vector2.zero;
                }

                // Jump, crouch sẽ do UI button gọi riêng (OnClick) nếu cần
                characterInputs.JumpDown = false;
                characterInputs.CrouchDown = false;
                characterInputs.CrouchUp = false;
            }

            // Camera rotation cho CharacterControl (dùng để tính cameraPlanarDirection)
            Camera cam = (characterControl != null && characterControl.ViewCamera != null)
                ? characterControl.ViewCamera
                : Camera.main;

            if (cam != null)
            {
                characterInputs.CameraRotation = cam.transform.rotation;
            }
            else
            {
                characterInputs.CameraRotation = Quaternion.identity;
            }

            // Apply inputs to character
            characterControl.SetInputs(ref characterInputs);
        }

        [Button]
        public void ActiveLookMouse(bool active)
        {
            characterControl.AlwaysLookAtMouse = active;
        }
    }
}
