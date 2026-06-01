namespace LearningArchitect.Modules.InterviewArena
{
    public interface IDamageable
    {
        CombatTeam Team { get; }
        bool IsAlive { get; }
        void ApplyDamage(in DamageInfo info);
    }
}
