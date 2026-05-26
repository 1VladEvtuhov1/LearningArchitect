namespace LearningArchitect.Modules.InterviewArena
{
    public static class EnemyFsmLogic
    {
        public static bool CanSeePlayer(
            float distanceToPlayer,
            float forwardDot,
            float detectRange,
            float visibilityDotThreshold)
        {
            if (distanceToPlayer > detectRange)
                return false;

            return forwardDot >= visibilityDotThreshold;
        }

        public static bool ShouldLosePlayer(float distanceToPlayer, float loseDetectRange)
        {
            return distanceToPlayer > loseDetectRange;
        }

        public static bool IsInCrossbowRange(float distance, float minRange, float maxRange)
        {
            return distance > minRange && distance <= maxRange;
        }

        public static EnemyState ResolveNextState(
            EnemyState current,
            bool canSeePlayer,
            bool shouldLosePlayer,
            bool inMeleeRange,
            bool inCrossbowRange,
            bool attackCycleActive,
            bool rangedCycleActive)
        {
            if (shouldLosePlayer)
                return EnemyState.Patrol;

            if (!canSeePlayer)
            {
                if (current == EnemyState.Attack && attackCycleActive)
                    return EnemyState.Attack;

                if (current == EnemyState.Ranged && rangedCycleActive)
                    return EnemyState.Ranged;

                return EnemyState.Patrol;
            }

            if (inMeleeRange)
                return EnemyState.Attack;

            if (inCrossbowRange)
                return EnemyState.Ranged;

            return EnemyState.Chase;
        }
    }
}
