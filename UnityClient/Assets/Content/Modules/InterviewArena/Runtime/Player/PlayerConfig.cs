using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [CreateAssetMenu(
        menuName = "Learning Architect/Interview Arena/Player Config",
        fileName = "InterviewArena_PlayerConfig")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 7.5f;
        [SerializeField] private float groundAcceleration = 52f;
        [SerializeField] private float groundDeceleration = 62f;
        [SerializeField] private float airAcceleration = 18f;
        [SerializeField] private float rotationSpeed = 12f;
        [SerializeField] private float airControl = 0.35f;
        [SerializeField] private float maxGroundAngle = 48f;

        [Header("Jump")]
        [SerializeField] private float jumpImpulse = 6.5f;
        [SerializeField] private float coyoteTime = 0.15f;
        [SerializeField] private float jumpCutMultiplier = 0.45f;
        [SerializeField] private float fallGravityMultiplier = 1.65f;
        [SerializeField] private float maxFallSpeed = 22f;

        [Header("Dash")]
        [SerializeField] private float dashImpulse = 11f;
        [SerializeField] private float dashCooldown = 1.1f;
        [SerializeField] private float dashDuration = 0.14f;

        [Header("Collision")]
        [SerializeField] private float airborneWallSlide = 0.35f;
        [SerializeField] private LayerMask obstructionMask = ~0;

        [Header("Ground Check")]
        [SerializeField] private float groundCheckRadius = 0.32f;
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private LayerMask groundMask = ~0;

        [Header("Arena Hover (Battlerite-style)")]
        [SerializeField] private bool useArenaHover = true;
        [SerializeField] private float hoverHeight = 0.14f;
        [SerializeField] private float hoverSpring = 120f;
        [SerializeField] private float hoverDamper = 18f;
        [SerializeField] private float hoverGravity = 34f;
        [SerializeField] private float maxVerticalSpeed = 12f;
        [SerializeField] private float probeRadius = 0.26f;
        [SerializeField] private float probeExtraDistance = 0.55f;
        [SerializeField] private float forwardProbeOffset = 0.45f;
        [SerializeField] private float sideProbeOffset = 0.25f;
        [SerializeField] private float maxStepHeight = 0.42f;
        [SerializeField] private float inputDeadZone = 0.05f;
        [SerializeField] private float aimTurnSpeed = 900f;

        public bool UseArenaHover => useArenaHover;
        public float MoveSpeed => moveSpeed;
        public float GroundAcceleration => groundAcceleration;
        public float GroundDeceleration => groundDeceleration;
        public float AirAcceleration => airAcceleration;
        public float RotationSpeed => rotationSpeed;
        public float AirControl => airControl;
        public float MaxGroundAngle => maxGroundAngle;
        public float JumpImpulse => jumpImpulse;
        public float CoyoteTime => coyoteTime;
        public float JumpCutMultiplier => jumpCutMultiplier;
        public float FallGravityMultiplier => fallGravityMultiplier;
        public float MaxFallSpeed => maxFallSpeed;
        public float AirborneWallSlide => airborneWallSlide;
        public LayerMask ObstructionMask => obstructionMask;
        public float DashImpulse => dashImpulse;
        public float DashCooldown => dashCooldown;
        public float DashDuration => dashDuration;
        public float GroundCheckRadius => groundCheckRadius;
        public float GroundCheckDistance => groundCheckDistance;
        public LayerMask GroundMask => groundMask;
        public float HoverHeight => hoverHeight;
        public float HoverSpring => hoverSpring;
        public float HoverDamper => hoverDamper;
        public float HoverGravity => hoverGravity;
        public float MaxVerticalSpeed => maxVerticalSpeed;
        public float ProbeRadius => probeRadius;
        public float ProbeExtraDistance => probeExtraDistance;
        public float ForwardProbeOffset => forwardProbeOffset;
        public float SideProbeOffset => sideProbeOffset;
        public float MaxStepHeight => maxStepHeight;
        public float InputDeadZone => inputDeadZone;
        public float AimTurnSpeed => aimTurnSpeed;
    }
}
