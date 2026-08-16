namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Pure routing for crossbow Consume vs TimedInputBuffer forgiveness.
    /// </summary>
    public static class PlayerCombatCrossbowInputRules
    {
        /// <summary>
        /// Active shot: always consume (combo attempt, including rejected early/late).
        /// Idle: consume only when gate allows starting a new attack.
        /// </summary>
        public static bool ShouldConsumeCrossbowPress(bool isShotActive, bool canAttack) =>
            isShotActive || canAttack;
    }
}
