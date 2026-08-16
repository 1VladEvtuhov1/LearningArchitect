namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Single next-strike queue for a linear melee combo. Does not own active strike index.
    /// </summary>
    public sealed class MeleeComboState
    {
        private bool hasQueuedStrike;
        private int queuedStrikeIndex = -1;

        public bool HasQueuedStrike => hasQueuedStrike;
        public int QueuedStrikeIndex => hasQueuedStrike ? queuedStrikeIndex : -1;

        public void Clear()
        {
            hasQueuedStrike = false;
            queuedStrikeIndex = -1;
        }

        /// <summary>
        /// Half-open window: comboInputStart &lt;= elapsed &lt; comboInputEnd.
        /// </summary>
        public bool IsInComboInputWindow(IComboChainStep step, float elapsedSeconds)
        {
            if (step == null || step.NextStrikeIndex < 0)
                return false;

            if (!step.HasValidComboTimings)
                return false;

            return elapsedSeconds >= step.ComboInputStart
                && elapsedSeconds < step.ComboInputEnd;
        }

        public bool TryQueue(
            IComboChainStep activeStep,
            float elapsedSeconds,
            int strikeCount,
            out int queuedIndex)
        {
            queuedIndex = -1;

            if (hasQueuedStrike)
                return false;

            if (activeStep == null || activeStep.NextStrikeIndex < 0)
                return false;

            if (!activeStep.HasValidComboTimings)
                return false;

            if (!IsInComboInputWindow(activeStep, elapsedSeconds))
                return false;

            int next = activeStep.NextStrikeIndex;
            if (next < 0 || next >= strikeCount)
                return false;

            hasQueuedStrike = true;
            queuedStrikeIndex = next;
            queuedIndex = next;
            return true;
        }

        public bool ShouldTransition(IComboChainStep activeStep, float elapsedSeconds)
        {
            if (!hasQueuedStrike || activeStep == null || activeStep.NextStrikeIndex < 0)
                return false;

            if (!activeStep.HasValidComboTimings)
                return false;

            return elapsedSeconds >= activeStep.ComboTransitionTime;
        }

        public bool TryConsumeQueue(out int strikeIndex)
        {
            if (!hasQueuedStrike)
            {
                strikeIndex = -1;
                return false;
            }

            strikeIndex = queuedStrikeIndex;
            Clear();
            return true;
        }
    }
}
