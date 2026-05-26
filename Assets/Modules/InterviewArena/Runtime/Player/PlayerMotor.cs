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
        private float coyoteTimer;
        private float dashCooldownTimer;
        private float dashTimer;
        private Vector3 dashDirection = Vector3.forward;
        private Vector3 cachedCameraForward = Vector3.forward;
        private Vector3 cachedCameraRight = Vector3.right;
        private float capsuleHalfHeight = 1f;

        public Transform ViewPivot => viewPivot != null ? viewPivot : transform;
        public bool IsGrounded => ground != null && ground.IsGrounded;
        public bool IsWalkable => ground != null && ground.IsWalkable;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            input = GetComponent<PlayerInputReader>();
            ground = GetComponent<GroundDetector>();

            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            if (movementCamera == null)
                movementCamera = Camera.main;

            if (viewPivot == null)
                viewPivot = transform;

            capsuleHalfHeight = InterviewArenaPlatformLayout.ResolveCapsuleHalfHeight(GetComponent<CapsuleCollider>());
            ResolveArenaFloorReference();

            if (config != null)
                ground.ApplyConfig(config, viewPivot);
        }

        public void ConfigureArenaFloor(Collider floorCollider)
        {
            arenaFloor = floorCollider;
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
        }

        public void ApplyConfig(PlayerConfig playerConfig, Camera camera)
        {
            config = playerConfig;
            movementCamera = camera != null ? camera : movementCamera;
            if (ground != null && config != null)
                ground.ApplyConfig(config, viewPivot);
        }

        private void Update()
        {
            if (config == null)
                return;

            RefreshCameraBasis();
            UpdateCoyoteTime();
            UpdateDashTimers();

            PlayerInputFrame frame = input.CurrentFrame;
            if (input.ConsumeDash() && dashCooldownTimer <= 0f && frame.Move.sqrMagnitude > 0.01f)
                BeginDash(frame.Move);
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
                SlideAlongWalls();
                ClampToArena();
                return;
            }

            ApplyPlanarMovement(input.CurrentFrame.Move);
            SlideAlongWalls();
            ClampToArena();
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
                coyoteTimer = config.CoyoteTime;
            else
                coyoteTimer = Mathf.Max(0f, coyoteTimer - Time.deltaTime);
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

            Vector3 targetPlanar = wishDirection * (config.MoveSpeed * control);
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

        private void ResolveArenaFloorReference()
        {
            if (arenaFloor != null)
                return;

            GameObject platform = GameObject.Find("PreviewPlatform");
            if (platform != null)
                arenaFloor = platform.GetComponent<Collider>();
        }

        private void ClampToArena()
        {
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
            CapsuleCollider capsule = GetComponent<CapsuleCollider>();
            if (capsule == null)
                return 0.35f;

            return Mathf.Max(0.35f, capsule.radius * capsule.transform.lossyScale.x + 0.1f);
        }

        private void SlideAlongWalls()
        {
            CapsuleCollider capsule = GetComponent<CapsuleCollider>();
            if (capsule == null || !ground.IsWalkable)
                return;

            Vector3 planarVelocity = new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z);
            if (planarVelocity.sqrMagnitude < 0.01f)
                return;

            ResolveCapsuleCastPoints(capsule, out Vector3 pointA, out Vector3 pointB, out float radius);
            Vector3 direction = planarVelocity.normalized;
            float castDistance = planarVelocity.magnitude * Time.fixedDeltaTime + 0.08f;

            if (!Physics.CapsuleCast(
                pointA,
                pointB,
                radius,
                direction,
                out RaycastHit hit,
                castDistance,
                InterviewArenaPhysicsLayers.GroundMask,
                QueryTriggerInteraction.Ignore))
            {
                return;
            }

            Vector3 wallNormal = hit.normal;
            wallNormal.y = 0f;
            if (wallNormal.sqrMagnitude < 0.0001f)
                return;

            wallNormal.Normalize();
            float intoWall = Vector3.Dot(planarVelocity, wallNormal);
            if (intoWall <= 0f)
                return;

            planarVelocity -= wallNormal * intoWall;
            body.linearVelocity = new Vector3(planarVelocity.x, body.linearVelocity.y, planarVelocity.z);
        }

        private static void ResolveCapsuleCastPoints(
            CapsuleCollider capsule,
            out Vector3 pointA,
            out Vector3 pointB,
            out float radius)
        {
            Transform capsuleTransform = capsule.transform;
            float scaleY = Mathf.Abs(capsuleTransform.lossyScale.y);
            float scaleXZ = Mathf.Max(
                Mathf.Abs(capsuleTransform.lossyScale.x),
                Mathf.Abs(capsuleTransform.lossyScale.z));
            radius = Mathf.Max(0.01f, capsule.radius * scaleXZ);
            float height = Mathf.Max(capsule.height * scaleY, radius * 2f + 0.01f);
            float cylinder = Mathf.Max(0f, height - radius * 2f);
            Vector3 center = capsuleTransform.TransformPoint(capsule.center);
            Vector3 up = capsuleTransform.up;
            pointA = center - up * (cylinder * 0.5f);
            pointB = center + up * (cylinder * 0.5f);
        }
    }
}
