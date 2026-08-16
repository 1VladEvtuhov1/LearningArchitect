namespace LearningArchitect.Modules.InterviewArena
{
    public interface IDamageable
    {
        CombatTeam Team { get; }
        bool IsAlive { get; }
        void ApplyDamage(in DamageInfo info);
    }

    /// <summary>
    /// Optional same-object absorb before Health applies HP / hitstun.
    /// </summary>
    public interface IIncomingHitGuard
    {
        bool TryAbsorb(in DamageInfo incoming, out DamageInfo absorbed);
    }
}
