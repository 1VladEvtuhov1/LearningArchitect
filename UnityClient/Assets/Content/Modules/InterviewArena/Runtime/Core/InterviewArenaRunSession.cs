using System;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Cross-scene session signals for the arena loop (finish portal, results UI later).
    /// </summary>
    public static class InterviewArenaRunSession
    {
        public static event Action RunCompleted;

        public static void NotifyRunCompleted()
        {
            RunCompleted?.Invoke();
        }
    }
}
