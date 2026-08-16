using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    public sealed class EnemySensor : MonoBehaviour
    {
        [SerializeField] private Transform awarenessOrigin;

        private Transform playerTarget;

        public Transform PlayerTarget => playerTarget;
        public Transform AwarenessOrigin => awarenessOrigin != null ? awarenessOrigin : transform;

        public void BindPlayer(Transform player)
        {
            playerTarget = player;
        }

        public bool TrySamplePlayer(EnemyConfig config, out float distance, out float forwardDot)
        {
            distance = float.MaxValue;
            forwardDot = -1f;

            if (config == null || playerTarget == null)
                return false;

            Transform origin = AwarenessOrigin;
            Vector3 toPlayer = playerTarget.position - origin.position;
            distance = toPlayer.magnitude;
            forwardDot = toPlayer.sqrMagnitude > 0.0001f
                ? Vector3.Dot(origin.forward, toPlayer.normalized)
                : 1f;

            return EnemyFsmLogic.CanSeePlayer(
                distance,
                forwardDot,
                config.DetectRange,
                config.VisibilityDotThreshold);
        }

        public bool IsInAttackRange(EnemyConfig config, out float distance)
        {
            distance = float.MaxValue;
            if (config == null || playerTarget == null)
                return false;

            distance = Vector3.Distance(AwarenessOrigin.position, playerTarget.position);
            return distance <= config.AttackRange;
        }

        public bool IsInCrossbowRange(EnemyConfig config, out float distance)
        {
            distance = float.MaxValue;
            if (config == null || playerTarget == null)
                return false;

            distance = Vector3.Distance(AwarenessOrigin.position, playerTarget.position);
            return EnemyFsmLogic.IsInCrossbowRange(
                distance,
                config.CrossbowMinRange,
                config.CrossbowMaxRange);
        }
    }
}
