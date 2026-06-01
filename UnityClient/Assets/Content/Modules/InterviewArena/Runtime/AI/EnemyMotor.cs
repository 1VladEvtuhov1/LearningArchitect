using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class EnemyMotor : MonoBehaviour
    {
        [SerializeField] private Transform facingPivot;

        private Rigidbody body;
        private CapsuleCollider capsule;
        private Vector3 patrolAnchor;
        private Vector3 patrolTarget;
        private float patrolIdleTimer;

        public void Initialize(Vector3 anchor)
        {
            patrolAnchor = anchor;
            PickNextPatrolTarget(0f);
        }

        public void TickPatrol(EnemyConfig config, float deltaTime)
        {
            if (config == null)
                return;

            patrolIdleTimer -= deltaTime;
            if (patrolIdleTimer > 0f)
            {
                StopPlanarMotion();
                return;
            }

            MoveTowards(patrolTarget, config.PatrolSpeed, config.RotationSpeed, deltaTime);
            if (PlanarDistanceTo(patrolTarget) <= 0.35f)
            {
                patrolIdleTimer = config.PatrolIdleDuration;
                PickNextPatrolTarget(config.PatrolRadius);
            }
        }

        public void TickChase(EnemyConfig config, Transform target, float deltaTime)
        {
            if (config == null || target == null)
                return;

            MoveTowards(target.position, config.ChaseSpeed, config.RotationSpeed, deltaTime);
        }

        public void FaceTarget(Transform target, EnemyConfig config, float deltaTime)
        {
            if (target == null || config == null)
                return;

            Vector3 toTarget = target.position - FacingOrigin.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            FacingOrigin.rotation = Quaternion.Slerp(
                FacingOrigin.rotation,
                targetRotation,
                config.RotationSpeed * deltaTime);
        }

        public void StopPlanarMotion()
        {
            if (body == null)
                return;

            Vector3 velocity = body.linearVelocity;
            body.linearVelocity = new Vector3(0f, velocity.y, 0f);
        }

        private Transform FacingOrigin => facingPivot != null ? facingPivot : transform;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            capsule = GetComponent<CapsuleCollider>();
            patrolAnchor = transform.position;
            PickNextPatrolTarget(3f);
        }

        private void PickNextPatrolTarget(float radius)
        {
            Vector2 disc = Random.insideUnitCircle * Mathf.Max(0.5f, radius);
            patrolTarget = patrolAnchor + new Vector3(disc.x, 0f, disc.y);
        }

        private void MoveTowards(Vector3 target, float speed, float rotationSpeed, float deltaTime)
        {
            Vector3 current = body.position;
            Vector3 toTarget = target - current;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f)
            {
                StopPlanarMotion();
                return;
            }

            Vector3 direction = toTarget.normalized;
            Vector3 planarVelocity = direction * speed;
            planarVelocity = SlidePlanarVelocity(planarVelocity);
            Vector3 velocity = body.linearVelocity;
            body.linearVelocity = new Vector3(planarVelocity.x, velocity.y, planarVelocity.z);

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            FacingOrigin.rotation = Quaternion.Slerp(
                FacingOrigin.rotation,
                targetRotation,
                rotationSpeed * deltaTime);
        }

        private float PlanarDistanceTo(Vector3 target)
        {
            Vector3 delta = target - body.position;
            delta.y = 0f;
            return delta.magnitude;
        }

        private Vector3 SlidePlanarVelocity(Vector3 planarVelocity)
        {
            if (capsule == null || !InterviewArenaPhysicsLayers.LayersConfigured)
                return planarVelocity;

            return PlanarLocomotionCollision.SlideAlongWalls(
                body,
                capsule,
                planarVelocity,
                InterviewArenaPhysicsLayers.GroundMask,
                airborneSlideScale: 0.5f);
        }
    }
}
