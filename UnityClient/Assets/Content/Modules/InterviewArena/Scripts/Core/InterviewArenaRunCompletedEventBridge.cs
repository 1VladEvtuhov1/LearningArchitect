using LearningArchitect.Shared.Events;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Raises a <see cref="GameEvent"/> when <see cref="InterviewArenaRunSession"/> completes a run.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InterviewArenaRunCompletedEventBridge : MonoBehaviour
    {
        [SerializeField] private GameEvent runCompletedEvent;

        private void OnEnable() => InterviewArenaRunSession.RunCompleted += HandleRunCompleted;

        private void OnDisable() => InterviewArenaRunSession.RunCompleted -= HandleRunCompleted;

        private void HandleRunCompleted()
        {
            if (runCompletedEvent != null)
                runCompletedEvent.Raise();
        }
    }
}
