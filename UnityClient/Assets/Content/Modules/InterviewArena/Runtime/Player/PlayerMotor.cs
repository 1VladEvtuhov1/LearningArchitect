using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(GroundDetector))]
    [DisallowMultipleComponent]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private PlayerConfig config;
        [SerializeField] private Transform viewPivot;
        [SerializeField] private Camera movementCamera;
        [SerializeField] private Collider arenaFloor;

        private Rigidbody body;
        private PlayerInputReader input;
        private GroundDetector ground;
        private PlayerBuffController buffController;
        private CapsuleCollider capsule;
        private float coyoteTimer;
        private float dashCooldownTimer;
        private float dashTimer;
        private Vector3 dashDirection = Vector3.forward;
        private Vector3 cachedCameraForward = Vector3.forward;
        private Vector3 cachedCameraRight = Vector3.right;
        private float capsuleHalfHeight = 1f;
        private bool loggedMissingArenaFloor;
        private bool jumpCutApplied;

        public Transform ViewPivot => viewPivot != null ? viewPivot : transform;
        public bool IsGrounded => ground != null && ground.IsGrounded;
        public bool IsWalkable => ground != null && ground.IsWalkable;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            input = GetComponent<PlayerInputReader>();
            ground = GetComponent<GroundDetector>();
            buffController = GetComponent<PlayerBuffController>();
            capsule = GetComponent<CapsuleCollider>();

            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            capsuleHalfHeight = InterviewArenaPlatformLayout.ResolveCapsuleHalfHeight(capsule);

            if (config != null && viewPivot != null)
                ground.ApplyConfig(config, viewPivot);
        }

        public void ConfigureArenaFloor(Collider floorCollider)
        {
            arenaFloor = floorCollider;
            loggedMissingArenaFloor = false;
        }

        public void SnapToSpawnPose(Vector3 planarPosition, Quaternion rotation, Collider floorCollider = null)
        {
            if (floorCollider != null)
                arenaFloor = floorCollider;

            Vector3 spawnPosition = InterviewArenaPlatformLayout.ResolveSpawnPosition(
                planarPosition,
                capsuleHalfHeight,
                arenaFloor);

            transform.SetPositionAndRotation(spawnPosition, rotation);
            if (body != null)
            {
                body.position = spawnPosition;
                body.rotation = rotation;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            jumpCutApplied = false;
        }

        public void ApplyConfig(PlayerConfig playerConfig, Camera camera)
        {
            config = playerConfig;
            movementCamera = camera;

            if (config == null)
                InterviewArenaAuthoringLog.MissingReference(this, nameof(config));
            if (viewPivot == null)
                InterviewArenaAuthoringLog.MissingReference(this, nameof(viewPivot));
            if (movementCamera == null)
                InterviewArenaAuthoringLog.MissingReference(this, nameof(movementCamera));
            if (ground != null && config != null && viewPivot != null)
                ground.ApplyConfig(config, viewPivot);
        }

        private void Update()
        {
            if (config == null)
                return;

            RefreshCameraBasis();
            UpdateCoyoteTime();
            UpdateDashTimers();
            ApplyVariableJumpCut();
            BufferDashFromInput();
        }

        private void FixedUpdate()
        {
            if (config == null)
                return;

            if (input.ConsumeJump() && coyoteTimer > 0f)
                Jump();

            if (dashTimer > 0f)
            {
                ApplyDashVelocity();
                ApplyWallSlide(ground.IsWalkable);
                ApplyFallGravity();
                ClampToArena();
                return;
            }

            ApplyPlanarMovement(input.CurrentFrame.Move);
            ApplyWallSlide(ground.IsWalkable);
            ApplyFallGravity();
            ClampToArena();
        }

        private void BufferDashFromInput()
        {
            PlayerInputFrame frame = input.CurrentFrame;
            if (input.ConsumeDash() && dashCooldownTimer <= 0f && frame.Move.sqrMagnitude > 0.01f)
                BeginDash(frame.Move);
        }

        private void ApplyVariableJumpCut()
        {
            if (input.JumpHeld || jumpCutApplied || body.linearVelocity.y <= 0f)
                return;

            float nextY = PlayerLocomotionMath.ApplyJumpCut(
                body.linearVelocity.y,
                config.JumpCutMultiplier);
            if (Mathf.Approximately(nextY, body.linearVelocity.y))
                return;

            jumpCutApplied = true;
            Vector3 velocity = body.linearVelocity;
            body.linearVelocity = new Vector3(velocity.x, nextY, velocity.z);
        }

        private void ApplyFallGravity()
        {
            if (ground.IsWalkable)
                return;

            float vertical = body.linearVelocity.y;
            if (vertical >= 0f)
                return;

            float extra = (config.FallGravityMultiplier - 1f) * Mathf.Abs(Physics.gravity.y);
            vertical -= extra * Time.fixedDeltaTime;
            vertical = Mathf.Max(vertical, -config.MaxFallSpeed);
            body.linearVelocity = new Vector3(body.linearVelocity.x, vertical, body.linearVelocity.z);
        }

        private void RefreshCameraBasis()
        {
            Transform cameraTransform = movementCamera != null ? movementCamera.transform : null;
            if (cameraTransform == null)
                return;

            cachedCameraForward = cameraTransform.forward;
            cachedCameraRight = cameraTransform.right;
        }

        private void UpdateCoyoteTime()
        {
            if (ground.IsWalkable)
            {
                coyoteTimer = config.CoyoteTime;
                jumpCutApplied = false;
            }
            else
            {
                coyoteTimer = Mathf.Max(0f, coyoteTimer - Time.deltaTime);
            }
        }

        private void UpdateDashTimers()
        {
            dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - Time.deltaTime);
            dashTimer = Mathf.Max(0f, dashTimer - Time.deltaTime);
        }

        private void BeginDash(Vector2 moveInput)
        {
            dashDirection = GetCameraRelativeDirection(moveInput);
            if (dashDirection.sqrMagnitude < 0.01f)
                dashDirection = viewPivot.forward;

            dashTimer = config.DashDuration;
            dashCooldownTimer = config.DashCooldown;
            jumpCutApplied = false;
            body.linearVelocity = new Vector3(
                dashDirection.x * config.DashImpulse,
                body.linearVelocity.y,
                dashDirection.z * config.DashImpulse);
        }

        private void ApplyDashVelocity()
        {
            body.linearVelocity = new Vector3(
                dashDirection.x * config.DashImpulse,
                body.linearVelocity.y,
                dashDirection.z * config.DashImpulse);
        }

        private void Jump()
        {
            coyoteTimer = 0f;
            jumpCutApplied = false;
            Vector3 velocity = body.linearVelocity;
            velocity.y = 0f;
            body.linearVelocity = velocity;
            body.AddForce(Vector3.up * config.JumpImpulse, ForceMode.Impulse);
        }

        private void ApplyPlanarMovement(Vector2 moveInput)
        {
            Vector3 wishDirection = GetCameraRelativeDirection(moveInput);
            bool grounded = ground.IsWalkable;
            float control = grounded ? 1f : config.AirControl;

            if (grounded)
                wishDirection = PlayerLocomotionMath.ProjectOnGround(wishDirection, ground.GroundNormal);

            float speedMultiplier = buffController != null
                ? buffController.GetMultiplier(BuffKind.MoveSpeed)
                : 1f;
            Vector3 targetPlanar = wishDirection * (config.MoveSpeed * control * speedMultiplier);
            Vector3 currentPlanar = new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z);
            float acceleration = ResolveAcceleration(currentPlanar, targetPlanar, grounded);
            Vector3 nextPlanar = Vector3.MoveTowards(currentPlanar, targetPlanar, acceleration * Time.fixedDeltaTime);

            body.linearVelocity = new Vector3(nextPlanar.x, body.linearVelocity.y, nextPlanar.z);

            Vector3 faceDirection = wishDirection.sqrMagnitude > 0.01f
                ? wishDirection
                : new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z);

            if (faceDirection.sqrMagnitude > 0.01f)
                ApplyFacingRotation(faceDirection.normalized);
        }

        private void ApplyFacingRotation(Vector3 faceDirection)
        {
            Quaternion targetRotation = Quaternion.LookRotation(faceDirection, Vector3.up);
            Quaternion nextRotation = Quaternion.Slerp(
                body.rotation,
                targetRotation,
                config.RotationSpeed * Time.fixedDeltaTime);
            body.MoveRotation(nextRotation);

            if (viewPivot != null && viewPivot != transform)
                viewPivot.rotation = nextRotation;
        }

        private float ResolveAcceleration(Vector3 currentPlanar, Vector3 targetPlanar, bool grounded)
        {
            if (!grounded)
                return config.AirAcceleration;

            return targetPlanar.sqrMagnitude > currentPlanar.sqrMagnitude
                ? config.GroundAcceleration
                : config.GroundDeceleration;
        }

        private Vector3 GetCameraRelativeDirection(Vector2 moveInput)
        {
            return PlayerLocomotionMath.BuildCameraRelativePlanarDirection(
                moveInput,
                cachedCameraForward,
                cachedCameraRight);
        }

        private void ApplyWallSlide(bool grounded)
        {
            if (capsule == null)
                return;

            Vector3 planar = new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z);
            float airborneScale = grounded ? 1f : config.AirborneWallSlide;
            LayerMask mask = config.ObstructionMask;
            Vector3 slid = PlanarLocomotionCollision.SlideAlongWalls(
                body,
                capsule,
                planar,
                mask,
                airborneScale);
            PlanarLocomotionCollision.ApplyPlanarVelocity(body, slid);
        }

        private void ClampToArena()
        {
            if (arenaFloor == null)
            {
                if (!loggedMissingArenaFloor)
                {
                    loggedMissingArenaFloor = true;
                    InterviewArenaAuthoringLog.MissingReference(this, nameof(arenaFloor));
                }

                return;
            }

            float edgeMargin = ResolveArenaEdgeMargin();
            Vector3 clamped = InterviewArenaPlatformLayout.ClampToSurface(
                body.position,
                capsuleHalfHeight,
                arenaFloor,
                edgeMargin);
            if ((clamped - body.position).sqrMagnitude < 0.000001f)
                return;

            body.position = clamped;
            Vector3 velocity = body.linearVelocity;
            if (velocity.y < 0f)
                velocity.y = 0f;

            Vector2 planarVelocity = new Vector2(velocity.x, velocity.z);
            Vector2 planarPosition = new Vector2(clamped.x, clamped.z);
            float maxRadius = InterviewArenaPlatformLayout.ResolvePlanarRadius(arenaFloor, edgeMargin);
            if (planarPosition.sqrMagnitude >= maxRadius * maxRadius * 0.98f)
            {
                Vector2 normal = planarPosition.normalized;
                float outward = Vector2.Dot(planarVelocity, normal);
                if (outward > 0f)
                    planarVelocity -= normal * outward;
            }

            body.linearVelocity = new Vector3(planarVelocity.x, velocity.y, planarVelocity.y);
        }

        private float ResolveArenaEdgeMargin()
        {
            if (capsule == null)
                return 0.35f;

            return Mathf.Max(0.35f, capsule.radius * capsule.transform.lossyScale.x + 0.1f);
        }
    }
}
