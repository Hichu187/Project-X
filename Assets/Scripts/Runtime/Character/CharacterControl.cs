using KinematicCharacterController;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public enum CharacterState
    {
        Default,
    }

    public enum OrientationMethod
    {
        TowardsCamera,
        TowardsMovement,
        TowardsMouse,
        TowardsJoystick,
    }

    public struct PlayerCharacterInputs
    {
        public float MoveAxisForward;
        public float MoveAxisRight;
        public Quaternion CameraRotation;
        public bool JumpDown;
        public bool CrouchDown;
        public bool CrouchUp;
        public Vector2 LookJoystick;
    }

    public struct AICharacterInputs
    {
        public Vector3 MoveVector;
        public Vector3 LookVector;
    }

    public enum BonusOrientationMethod
    {
        None,
        TowardsGravity,
        TowardsGroundSlopeAndGravity,
    }

    public class CharacterControl : MonoBehaviour, ICharacterController
    {
        public KinematicCharacterMotor Motor;

        public float MaxStableMoveSpeed = 10f;
        public float StableMovementSharpness = 15f;
        public float OrientationSharpness = 10f;
        public OrientationMethod OrientationMethod = OrientationMethod.TowardsCamera;

        public bool AlwaysLookAtMouse = true;
        public Camera ViewCamera;

        public float MaxAirMoveSpeed = 15f;
        public float AirAccelerationSpeed = 15f;
        public float Drag = 0.1f;

        public bool AllowJumpingWhenSliding = false;
        public float JumpUpSpeed = 10f;
        public float JumpScalableForwardSpeed = 10f;
        public float JumpPreGroundingGraceTime = 0f;
        public float JumpPostGroundingGraceTime = 0f;

        public List<Collider> IgnoredColliders = new List<Collider>();
        public BonusOrientationMethod BonusOrientationMethod = BonusOrientationMethod.None;
        public float BonusOrientationSharpness = 10f;
        public Vector3 Gravity = new Vector3(0, -30f, 0);
        public Transform MeshRoot;
        public Transform CameraFollowPoint;
        public float CrouchedCapsuleHeight = 1f;

        public float AimHeightOffset = 0f;

        public CharacterState CurrentCharacterState { get; private set; }

        private Collider[] _probedColliders = new Collider[8];
        private Vector3 _moveInputVector;
        private Vector3 _lookInputVector;
        private bool _jumpRequested = false;
        private bool _jumpConsumed = false;
        private bool _jumpedThisFrame = false;
        private float _timeSinceJumpRequested = Mathf.Infinity;
        private float _timeSinceLastAbleToJump = 0f;
        private Vector3 _internalVelocityAdd = Vector3.zero;
        private bool _shouldBeCrouching = false;
        private bool _isCrouching = false;
        private Vector2 _joystickLook;

        private void Awake()
        {
            TransitionToState(CharacterState.Default);
            Motor.CharacterController = this;
        }

        public void TransitionToState(CharacterState newState)
        {
            CharacterState old = CurrentCharacterState;
            OnStateExit(old, newState);
            CurrentCharacterState = newState;
            OnStateEnter(newState, old);
        }

        public void OnStateEnter(CharacterState state, CharacterState from) { }
        public void OnStateExit(CharacterState state, CharacterState to) { }

        public void SetInputs(ref PlayerCharacterInputs inputs)
        {
            Vector3 moveInputVector = Vector3.ClampMagnitude(
                new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);

            Vector3 worldUp = Vector3.up;

            Vector3 cameraPlanarDirection =
                Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, worldUp).normalized;

            if (cameraPlanarDirection.sqrMagnitude == 0f)
            {
                cameraPlanarDirection =
                    Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, worldUp).normalized;
            }

            Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, worldUp);

            _joystickLook = inputs.LookJoystick;

            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        _moveInputVector = cameraPlanarRotation * moveInputVector;

                        switch (OrientationMethod)
                        {
                            case OrientationMethod.TowardsCamera:
                                _lookInputVector = cameraPlanarDirection;
                                break;

                            case OrientationMethod.TowardsMovement:
                                {
                                    if (_moveInputVector.sqrMagnitude > 0.0001f)
                                        _lookInputVector = _moveInputVector.normalized;
                                    else if (_lookInputVector.sqrMagnitude < 0.0001f)
                                        _lookInputVector = cameraPlanarDirection;
                                    break;
                                }

                            case OrientationMethod.TowardsMouse:
                                {
                                    if (AlwaysLookAtMouse && TryGetMouseWorldLookDirection(out var mouseDir))
                                        _lookInputVector = mouseDir;
                                    else if (_moveInputVector.sqrMagnitude > 0.0001f)
                                        _lookInputVector = _moveInputVector.normalized;
                                    else if (_lookInputVector.sqrMagnitude < 0.0001f)
                                        _lookInputVector = cameraPlanarDirection;
                                    break;
                                }

                            case OrientationMethod.TowardsJoystick:
                                {
                                    if (_joystickLook.sqrMagnitude > 0.0001f)
                                    {
                                        Vector3 camForward = cameraPlanarDirection;
                                        Vector3 camRight = Vector3.Cross(worldUp, camForward);

                                        Vector3 lookDir =
                                            (camRight * _joystickLook.x + camForward * _joystickLook.y).normalized;

                                        if (lookDir.sqrMagnitude > 0.0001f)
                                            _lookInputVector = lookDir;
                                    }
                                    else
                                    {
                                        if (_moveInputVector.sqrMagnitude > 0.0001f)
                                            _lookInputVector = _moveInputVector.normalized;
                                        else if (_lookInputVector.sqrMagnitude < 0.0001f)
                                            _lookInputVector = cameraPlanarDirection;
                                    }
                                    break;
                                }
                        }

                        if (inputs.JumpDown)
                        {
                            _timeSinceJumpRequested = 0f;
                            _jumpRequested = true;
                        }

                        if (inputs.CrouchDown)
                        {
                            _shouldBeCrouching = true;

                            if (!_isCrouching)
                            {
                                _isCrouching = true;
                                Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
                                if (MeshRoot != null)
                                    MeshRoot.localScale = new Vector3(1f, 0.5f, 1f);
                            }
                        }
                        else if (inputs.CrouchUp)
                        {
                            _shouldBeCrouching = false;
                        }

                        break;
                    }
            }
        }

        public void SetInputs(ref AICharacterInputs inputs)
        {
            _moveInputVector = inputs.MoveVector;
            _lookInputVector = inputs.LookVector;
        }

        private bool TryGetMouseWorldLookDirection(out Vector3 worldDirection)
        {
            worldDirection = Vector3.zero;

            Camera cam = ViewCamera != null ? ViewCamera : Camera.main;
            if (cam == null)
                return false;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            Vector3 worldUp = Vector3.up;
            Vector3 aimOrigin = Motor.TransientPosition + worldUp * AimHeightOffset;

            Plane plane = new Plane(worldUp, aimOrigin);

            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);

                Vector3 dir = hitPoint - aimOrigin;
                dir = Vector3.ProjectOnPlane(dir, worldUp).normalized;

                if (dir.sqrMagnitude > 0.0001f)
                {
                    worldDirection = dir;
                    return true;
                }
            }

            return false;
        }

        public void BeforeCharacterUpdate(float deltaTime) { }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        if (_lookInputVector.sqrMagnitude > 0f && OrientationSharpness > 0f)
                        {
                            Vector3 smoothedLookInputDirection = Vector3.Slerp(
                                Motor.CharacterForward,
                                _lookInputVector,
                                1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

                            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                        }

                        Vector3 currentUp = currentRotation * Vector3.up;

                        if (BonusOrientationMethod == BonusOrientationMethod.TowardsGravity)
                        {
                            Vector3 smoothedGravity =
                                Vector3.Slerp(currentUp, -Gravity.normalized,
                                    1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));

                            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravity) * currentRotation;
                        }
                        else if (BonusOrientationMethod == BonusOrientationMethod.TowardsGroundSlopeAndGravity)
                        {
                            if (Motor.GroundingStatus.IsStableOnGround)
                            {
                                Vector3 bottom =
                                    Motor.TransientPosition + currentUp * Motor.Capsule.radius;

                                Vector3 smoothedGround =
                                    Vector3.Slerp(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal,
                                        1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));

                                currentRotation =
                                    Quaternion.FromToRotation(currentUp, smoothedGround) * currentRotation;

                                Motor.SetTransientPosition(
                                    bottom + currentRotation * Vector3.down * Motor.Capsule.radius);
                            }
                            else
                            {
                                Vector3 smoothedGravity =
                                    Vector3.Slerp(currentUp, -Gravity.normalized,
                                        1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));

                                currentRotation =
                                    Quaternion.FromToRotation(currentUp, smoothedGravity) * currentRotation;
                            }
                        }
                        else
                        {
                            Vector3 smoothedUp =
                                Vector3.Slerp(currentUp, Vector3.up,
                                    1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));

                            currentRotation = Quaternion.FromToRotation(currentUp, smoothedUp) * currentRotation;
                        }

                        break;
                    }
            }
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            float velMagnitude = currentVelocity.magnitude;

                            Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;

                            currentVelocity =
                                Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) *
                                velMagnitude;

                            Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
                            Vector3 reorientedInput =
                                Vector3.Cross(effectiveGroundNormal, inputRight).normalized *
                                _moveInputVector.magnitude;
                            Vector3 targetVel = reorientedInput * MaxStableMoveSpeed;

                            currentVelocity =
                                Vector3.Lerp(currentVelocity, targetVel,
                                    1f - Mathf.Exp(-StableMovementSharpness * deltaTime));
                        }
                        else
                        {
                            if (_moveInputVector.sqrMagnitude > 0f)
                            {
                                Vector3 addedVel = _moveInputVector * AirAccelerationSpeed * deltaTime;

                                Vector3 velOnPlane =
                                    Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);

                                if (velOnPlane.magnitude < MaxAirMoveSpeed)
                                {
                                    Vector3 newVel =
                                        Vector3.ClampMagnitude(velOnPlane + addedVel, MaxAirMoveSpeed);
                                    addedVel = newVel - velOnPlane;
                                }
                                else
                                {
                                    if (Vector3.Dot(velOnPlane, addedVel) > 0f)
                                        addedVel = Vector3.ProjectOnPlane(addedVel, velOnPlane.normalized);
                                }

                                if (Motor.GroundingStatus.FoundAnyGround)
                                {
                                    if (Vector3.Dot(currentVelocity + addedVel, addedVel) > 0f)
                                    {
                                        Vector3 obstruct =
                                            Vector3.Cross(
                                                Vector3.Cross(Motor.CharacterUp,
                                                    Motor.GroundingStatus.GroundNormal),
                                                Motor.CharacterUp).normalized;
                                        addedVel =
                                            Vector3.ProjectOnPlane(addedVel, obstruct);
                                    }
                                }

                                currentVelocity += addedVel;
                            }

                            currentVelocity += Gravity * deltaTime;

                            currentVelocity *= 1f / (1f + Drag * deltaTime);
                        }

                        _jumpedThisFrame = false;
                        _timeSinceJumpRequested += deltaTime;

                        if (_jumpRequested)
                        {
                            bool canJump =
                                (!_jumpConsumed &&
                                 ((AllowJumpingWhenSliding
                                         ? Motor.GroundingStatus.FoundAnyGround
                                         : Motor.GroundingStatus.IsStableOnGround)
                                  || _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime));

                            if (canJump)
                            {
                                Vector3 jumpDir = Motor.CharacterUp;

                                if (Motor.GroundingStatus.FoundAnyGround &&
                                    !Motor.GroundingStatus.IsStableOnGround)
                                    jumpDir = Motor.GroundingStatus.GroundNormal;

                                Motor.ForceUnground();

                                currentVelocity += (jumpDir * JumpUpSpeed) -
                                                   Vector3.Project(currentVelocity, Motor.CharacterUp);

                                currentVelocity += _moveInputVector * JumpScalableForwardSpeed;

                                _jumpRequested = false;
                                _jumpConsumed = true;
                                _jumpedThisFrame = true;
                            }
                        }

                        if (_internalVelocityAdd.sqrMagnitude > 0f)
                        {
                            currentVelocity += _internalVelocityAdd;
                            _internalVelocityAdd = Vector3.zero;
                        }

                        break;
                    }
            }
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        if (_jumpRequested && _timeSinceJumpRequested > JumpPreGroundingGraceTime)
                            _jumpRequested = false;

                        if (AllowJumpingWhenSliding
                                ? Motor.GroundingStatus.FoundAnyGround
                                : Motor.GroundingStatus.IsStableOnGround)
                        {
                            if (!_jumpedThisFrame)
                                _jumpConsumed = false;

                            _timeSinceLastAbleToJump = 0f;
                        }
                        else
                        {
                            _timeSinceLastAbleToJump += deltaTime;
                        }

                        if (_isCrouching && !_shouldBeCrouching)
                        {
                            Motor.SetCapsuleDimensions(0.5f, 2f, 1f);

                            if (Motor.CharacterOverlap(
                                    Motor.TransientPosition,
                                    Motor.TransientRotation,
                                    _probedColliders,
                                    Motor.CollidableLayers,
                                    QueryTriggerInteraction.Ignore) > 0)
                            {
                                Motor.SetCapsuleDimensions(
                                    0.5f,
                                    CrouchedCapsuleHeight,
                                    CrouchedCapsuleHeight * 0.5f);
                            }
                            else
                            {
                                if (MeshRoot != null)
                                    MeshRoot.localScale = new Vector3(1f, 1f, 1f);

                                _isCrouching = false;
                            }
                        }

                        break;
                    }
            }
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            if (Motor.GroundingStatus.IsStableOnGround &&
                !Motor.LastGroundingStatus.IsStableOnGround)
                OnLanded();
            else if (!Motor.GroundingStatus.IsStableOnGround &&
                     Motor.LastGroundingStatus.IsStableOnGround)
                OnLeaveStableGround();
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            if (IgnoredColliders.Count == 0)
                return true;

            if (IgnoredColliders.Contains(coll))
                return false;

            return true;
        }

        public void OnGroundHit(Collider hitCollider,
            Vector3 hitNormal,
            Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        { }

        public void OnMovementHit(Collider hitCollider,
            Vector3 hitNormal,
            Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        { }

        public void AddVelocity(Vector3 velocity)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    _internalVelocityAdd += velocity;
                    break;
            }
        }

        public Vector3 GetInputMoveVector()
        {
            return _moveInputVector;
        }

        public void ProcessHitStabilityReport(Collider hitCollider,
            Vector3 hitNormal,
            Vector3 hitPoint,
            Vector3 atCharacterPosition,
            Quaternion atCharacterRotation,
            ref HitStabilityReport hitStabilityReport)
        { }

        protected void OnLanded() { }
        protected void OnLeaveStableGround() { }
        public void OnDiscreteCollisionDetected(Collider hitCollider) { }
    }
}
