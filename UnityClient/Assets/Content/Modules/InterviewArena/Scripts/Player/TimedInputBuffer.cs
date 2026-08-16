namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Single-action input buffer with decay (input forgiveness window).
    /// </summary>
    public struct TimedInputBuffer
    {
        private float remainingSeconds;

        public bool IsBuffered => remainingSeconds > 0f;

        public void Press(float bufferDurationSeconds)
        {
            if (bufferDurationSeconds <= 0f)
                return;

            remainingSeconds = bufferDurationSeconds;
        }

        public void Tick(float deltaTimeSeconds)
        {
            if (remainingSeconds <= 0f)
                return;

            remainingSeconds -= deltaTimeSeconds;
            if (remainingSeconds < 0f)
                remainingSeconds = 0f;
        }

        public bool Consume()
        {
            if (remainingSeconds <= 0f)
                return false;

            remainingSeconds = 0f;
            return true;
        }
    }
}
