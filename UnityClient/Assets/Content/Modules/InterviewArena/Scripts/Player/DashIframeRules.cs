namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Middle coverage of a dash is invulnerable; equal edges stay open.
    /// </summary>
    public static class DashIframeRules
    {
        public const float DefaultCoverage = 0.85f;

        public static bool IsActive(float elapsed, float duration, float coverage)
        {
            if (duration <= 0f || elapsed < 0f || elapsed > duration)
                return false;

            if (coverage <= 0f)
                return false;

            if (coverage > 1f)
                coverage = 1f;

            float normalized = elapsed / duration;
            float edge = (1f - coverage) * 0.5f;
            return normalized >= edge && normalized <= 1f - edge;
        }
    }
}
