using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Pure helpers for the screen-space player health bar.
    /// </summary>
    public static class PlayerVitalHudRules
    {
        public static float Normalized(float current, float max)
        {
            if (max <= 0f)
                return 0f;

            return Mathf.Clamp01(current / max);
        }

        public static string FormatLabel(float current, float max)
        {
            int shownCurrent = Mathf.Max(0, Mathf.CeilToInt(current));
            int shownMax = Mathf.Max(0, Mathf.CeilToInt(max));
            return shownCurrent + "/" + shownMax;
        }
    }
}
