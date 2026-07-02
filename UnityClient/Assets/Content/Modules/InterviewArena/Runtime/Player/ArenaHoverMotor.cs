using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [RequireComponent(typeof(Rigidbody))]
    [DefaultExecutionOrder(20)]
    [DisallowMultipleComponent]
    public sealed class ArenaHoverMotor : MonoBehaviour
    {
        [SerializeField] private PlayerConfig config;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private Transform hoverAnchor;
        [SerializeField] private Collider arenaFloor;

        private Rigidbody body;
        private PlayerMotor playerMotor;
        private PlayerBuffController buffController;
        private CapsuleCollider capsule;

        public bool IsGrounded { get; private set; }

        private Vector3 Velocity
        {
            get => body.linearVelocity;
            set => body.linearVelocity = value;
        }

        private Vector3 AnchorPosition => hoverAnchor != null ? hoverAnchor.position : body.position;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            playerMotor = GetComponent<PlayerMotor>();
            buffController = GetComponent<PlayerBuffController>();
            capsule = GetComponent<CapsuleCollider>();

            if (inputReader == null)
                inputReader = GetComponent<PlayerInputReader>();

            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.constraints = RigidbodyConstraints.FreezeRotation;
        }

        public void ApplyConfig(PlayerConfig playerConfig, Camera camera, Transform anchor)
        {
            config = playerConfig;
            cameraRoot = camera != null ? camera.transform : null;
            if (anchor != null)
                hoverAnchor = anchor;
        }

        public void ConfigureArenaFloor(Collider floorCollider)
        {
            arenaFloor = floorCollider;
        }

        private void FixedUpdate()
        {
            if (config == null || !config.UseArenaHover || inputReader == null)
                return;

            Vector2 move = inputReader.CurrentFrame.Move;
            Vector3 wishDirection = BuildWishDirection(move);
            bool suppressGroundProbe = playerMotor != null && playerMotor.SuppressHoverGroundProbe;
            GroundInfo ground = suppressGroundProbe
                ? GroundInfo.Empty
                : ProbeGround(wishDirection);

            IsGrounded = ground.IsWalkable &&
                Mathf.Abs(Vector3.Dot(Velocity, Vector3.up)) < config.MaxVerticalSpeed * 0.35f;

            ApplyHover(ground);

            if (playerMotor == null || !playerMotor.IsDashing)
                ApplyMovement(wishDirection, ground, move);
            ApplyWallSlide(ground.IsWalkable);
            ClampVerticalSpeed();
            ClampToArena();
        }

        private Vector3 BuildWishDirection(Vector2 move)
        {
            if (move.sqrMagnitude < config.InputDeadZone * config.InputDeadZone)
                return Vector3.zero;

            Vector3 forward;
            Vector3 right;

            if (cameraRoot != null)
            {
                forward = Vector3.ProjectOnPlane(cameraRoot.forward, Vector3.up).normalized;
                right = Vector3.ProjectOnPlane(cameraRoot.right, Vector3.up).normalized;
            }
            else
            {
                forward = Vector3.forward;
                right = Vector3.right;
            }

            Vector3 direction = right * move.x + forward * move.y;
            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }

        private GroundInfo ProbeGround(Vector3 wishDirection)
        {
            Vector3 moveDirection = wishDirection.sqrMagnitude > 0.001f
                ? wishDirection.normalized
                : transform.forward;

            Vector3 sideDirection = Vector3.Cross(Vector3.up, moveDirection).normalized;

            Vector3[] offsets =
            {
                Vector3.zero,
                moveDirection * config.ForwardProbeOffset,
                sideDirection * config.SideProbeOffset,
                -sideDirection * config.SideProbeOffset
            };

            bool hasBestHit = false;
            GroundInfo bestHit = default;

            float castStartHeight = config.MaxStepHeight;
            float castDistance = config.MaxStepHeight + config.HoverHeight + config.ProbeExtraDistance;
            float minValidDistance = config.HoverHeight - config.MaxStepHeight;
            float maxValidDistance = config.HoverHeight + config.ProbeExtraDistance;

            foreach (Vector3 offset in offsets)
            {
                Vector3 origin = AnchorPosition + offset + Vector3.up * castStartHeight;

                bool hit = Physics.SphereCast(
                    origin,
                    config.ProbeRadius,
                    Vector3.down,
                    out RaycastHit hitInfo,
                    castDistance,
                    config.GroundMask,
                    QueryTriggerInteraction.Ignore);

                if (!hit)
                    continue;

                float slopeAngle = Vector3.Angle(hitInfo.normal, Vector3.up);
                if (slopeAngle > config.MaxGroundAngle)
                    continue;

                float verticalDistance = AnchorPosition.y - hitInfo.point.y;
                if (verticalDistance < minValidDistance || verticalDistance > maxValidDistance)
                    continue;

                GroundInfo current = new GroundInfo(
                    isWalkable: true,
                    point: hitInfo.point,
                    normal: hitInfo.normal,
                    verticalDistance: verticalDistance);

                if (!hasBestHit || current.Point.y > bestHit.Point.y)
                {
                    bestHit = current;
                    hasBestHit = true;
                }
            }

            return hasBestHit ? bestHit : GroundInfo.Empty;
        }

        private void ApplyHover(GroundInfo ground)
        {
            if (!ground.IsWalkable)
            {
                body.AddForce(Vector3.down * config.HoverGravity, ForceMode.Acceleration);
                return;
            }

            float heightError = config.HoverHeight - ground.VerticalDistance;
            float verticalSpeed = Vector3.Dot(Velocity, Vector3.up);
            float springAcceleration =
                heightError * config.HoverSpring -
                verticalSpeed * config.HoverDamper;

            body.AddForce(Vector3.up * springAcceleration, ForceMode.Acceleration);
        }

        private void ApplyMovement(Vector3 wishDirection, GroundInfo ground, Vector2 moveInput)
        {
            Vector3 planeNormal = ground.IsWalkable ? ground.Normal : Vector3.up;
            float speedMultiplier = buffController != null
                ? buffController.GetMultiplier(BuffKind.MoveSpeed)
                : 1f;

            Vector3 desiredVelocity = Vector3.zero;
            if (wishDirection.sqrMagnitude > 0.001f)
            {
                desiredVelocity = Vector3.ProjectOnPlane(wishDirection, planeNormal).normalized;
                desiredVelocity *= config.MoveSpeed * speedMultiplier;
            }

            Vector3 currentVelocityOnPlane = Vector3.ProjectOnPlane(Velocity, planeNormal);
            Vector3 velocityDelta = desiredVelocity - currentVelocityOnPlane;

            float acceleration = desiredVelocity.sqrMagnitude > 0.001f
                ? ground.IsWalkable ? config.GroundAcceleration : config.AirAcceleration
                : config.GroundDeceleration;

            velocityDelta = Vector3.ClampMagnitude(
                velocityDelta,
                acceleration * Time.fixedDeltaTime);

            body.AddForce(velocityDelta, ForceMode.VelocityChange);
        }

        private void ApplyWallSlide(bool grounded)
        {
            if (capsule == null || config == null)
                return;

            Vector3 planar = new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z);
            float airborneScale = grounded ? 1f : config.AirborneWallSlide;
            Vector3 slid = PlanarLocomotionCollision.SlideAlongWalls(
                body,
                capsule,
                planar,
                config.ObstructionMask,
                airborneScale);
            PlanarLocomotionCollision.ApplyPlanarVelocity(body, slid);
        }

        private void ClampVerticalSpeed()
        {
            Vector3 velocity = Velocity;
            float verticalSpeed = Vector3.Dot(velocity, Vector3.up);
            if (Mathf.Abs(verticalSpeed) <= config.MaxVerticalSpeed)
                return;

            Vector3 planarVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
            Vector3 clampedVerticalVelocity =
                Vector3.up * Mathf.Sign(verticalSpeed) * config.MaxVerticalSpeed;

            Velocity = planarVelocity + clampedVerticalVelocity;
        }

        private void ClampToArena()
        {
            if (arenaFloor == null)
                return;

            float edgeMargin = capsule != null
                ? Mathf.Max(0.35f, capsule.radius * capsule.transform.lossyScale.x + 0.1f)
                : 0.35f;

            // Hover owns the vertical axis (ApplyHover + ClampVerticalSpeed); this clamp only keeps
            // the body inside the arena radius so it never fights the hover spring on Y.
            float maxRadius = InterviewArenaPlatformLayout.ResolvePlanarRadius(arenaFloor, edgeMargin);
            Vector3 position = body.position;
            Vector2 planarPosition = new Vector2(position.x, position.z);
            if (planarPosition.sqrMagnitude <= maxRadius * maxRadius)
                return;

            Vector2 clampedPlanar = planarPosition.normalized * maxRadius;
            body.position = new Vector3(clampedPlanar.x, position.y, clampedPlanar.y);

            Vector3 velocity = body.linearVelocity;
            Vector2 planarVelocity = new Vector2(velocity.x, velocity.z);
            Vector2 outwardNormal = clampedPlanar.normalized;
            float outward = Vector2.Dot(planarVelocity, outwardNormal);
            if (outward > 0f)
                planarVelocity -= outwardNormal * outward;

            body.linearVelocity = new Vector3(planarVelocity.x, velocity.y, planarVelocity.y);
        }

        private readonly struct GroundInfo
        {
            public static readonly GroundInfo Empty = new GroundInfo(
                isWalkable: false,
                point: Vector3.zero,
                normal: Vector3.up,
                verticalDistance: 0f);

            public readonly bool IsWalkable;
            public readonly Vector3 Point;
            public readonly Vector3 Normal;
            public readonly float VerticalDistance;

            public GroundInfo(
                bool isWalkable,
                Vector3 point,
                Vector3 normal,
                float verticalDistance)
            {
                IsWalkable = isWalkable;
                Point = point;
                Normal = normal;
                VerticalDistance = verticalDistance;
            }
        }
    }
}
