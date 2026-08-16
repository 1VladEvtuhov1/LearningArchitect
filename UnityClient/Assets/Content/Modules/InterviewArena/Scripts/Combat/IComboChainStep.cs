namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Shared combo chain timings for melee strikes and crossbow shots.
    /// </summary>
    public interface IComboChainStep
    {
        int NextStrikeIndex { get; }
        bool HasValidComboTimings { get; }
        float ComboInputStart { get; }
        float ComboInputEnd { get; }
        float ComboTransitionTime { get; }
    }
}
