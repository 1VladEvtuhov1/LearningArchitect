namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Pure routing for melee Consume vs TimedInputBuffer forgiveness.
    /// </summary>
    public static class PlayerCombatMeleeInputRules
    {
        /// <summary>
        /// Active strike: always consume (combo attempt, including rejected early/late).
        /// Idle: consume only when gate allows starting a new attack (forgiveness preserved otherwise).
        /// </summary>
        public static bool ShouldConsumeMeleePress(
            bool isStrikeActive,
            bool canAttack,
            bool isBlocking = false) =>
            isStrikeActive || canAttack || isBlocking;
    }
}
