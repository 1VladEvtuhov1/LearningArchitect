namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Local player death: short plant, then restore at spawn. Online matches stay with MatchReporter.
    /// </summary>
    public static class PlayerDeathRespawnRules
    {
        public const float DefaultDelaySeconds = 1.6f;

        public static bool ShouldLocalRespawn(bool hasActiveOnlineMatch) => !hasActiveOnlineMatch;

        public static float ResolveDelay(float configuredDelay)
        {
            return configuredDelay > 0.05f ? configuredDelay : DefaultDelaySeconds;
        }

        public static bool TickDue(ref float timer, float deltaTime)
        {
            if (timer < 0f)
                return false;

            if (deltaTime < 0f)
                deltaTime = 0f;

            timer -= deltaTime;
            if (timer > 0f)
                return false;

            timer = -1f;
            return true;
        }
    }
}
