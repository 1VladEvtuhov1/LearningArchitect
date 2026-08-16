namespace LearningArchitect.Modules.InterviewArena
{
    public interface IPlayerActionGate
    {
        bool CanMove { get; }
        bool CanTurn { get; }
        bool CanAttack { get; }
        bool CanDash { get; }
        bool CanJump { get; }

        /// <summary>
        /// True while any action lock is active (dash/jump/melee/hitstun).
        /// </summary>
        bool HasActiveAction { get; }
    }
}
