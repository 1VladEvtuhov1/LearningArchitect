namespace LearningArchitect.Modules.InterviewArena
{
    public interface IPlayerActionGate
    {
        bool CanMove { get; }
        bool CanTurn { get; }
        bool CanAttack { get; }
        bool CanDash { get; }
        bool CanJump { get; }
        bool HasActiveAction { get; }
    }
}
