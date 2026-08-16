using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    internal static class InterviewArenaAuthoringLog
    {
        public static void MissingReference(Object context, string fieldName)
        {
            Debug.LogError(
                $"[InterviewArena] Missing required reference '{fieldName}'. Assign it in the Inspector (no runtime scene search).",
                context);
        }
    }
}
