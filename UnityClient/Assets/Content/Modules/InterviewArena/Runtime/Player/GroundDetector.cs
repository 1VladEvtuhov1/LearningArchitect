using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    public sealed class GroundDetector : MonoBehaviour
    {
        [SerializeField] private PlayerConfig config;
        [SerializeField] private Transform probeOrigin;

        private readonly PhysicsQueryService queryService = new PhysicsQueryService();

        public bool IsGrounded { get; private set; }
        public bool IsWalkable { get; private set; }
        public Vector3 GroundNormal { get; private set; } = Vector3.up;

        public void ApplyConfig(PlayerConfig playerConfig, Transform origin)
        {
            config = playerConfig;
            probeOrigin = origin;
        }

        private void FixedUpdate()
        {
            if (config == null)
            {
                IsGrounded = false;
                IsWalkable = false;
                GroundNormal = Vector3.up;
                return;
            }

            Transform originTransform = probeOrigin != null ? probeOrigin : transform;
            Vector3 origin = originTransform.position;
            float distance = config.GroundCheckDistance + config.GroundCheckRadius;

            IsGrounded = queryService.TrySphereCastDown(
                origin,
                config.GroundCheckRadius,
                distance,
                config.GroundMask,
                out RaycastHit hit,
                QueryTriggerInteraction.Collide);

            GroundNormal = IsGrounded ? hit.normal : Vector3.up;
            IsWalkable = IsGrounded && PlayerLocomotionMath.IsWalkableNormal(GroundNormal, config.MaxGroundAngle);
        }
    }
}
