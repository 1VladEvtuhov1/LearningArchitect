using System;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Cross-scene session signals for the arena loop (finish portal, results UI later).
    /// </summary>
    public static class InterviewArenaRunSession
    {
        public static event Action RunCompleted;
        public static event Action PlayerDied;
        public static event Action PlayerRespawned;

        public static void NotifyRunCompleted()
        {
            RunCompleted?.Invoke();
        }

        public static void NotifyPlayerDied()
        {
            PlayerDied?.Invoke();
        }

        public static void NotifyPlayerRespawned()
        {
            PlayerRespawned?.Invoke();
        }
    }
}
