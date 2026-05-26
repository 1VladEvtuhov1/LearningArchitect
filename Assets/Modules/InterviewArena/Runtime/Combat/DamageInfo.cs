using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    public readonly struct DamageInfo
    {
        public DamageInfo(
            float amount,
            CombatTeam sourceTeam,
            Vector3 hitPoint,
            Vector3 hitDirection,
            float knockbackImpulse = 0f)
        {
            Amount = amount;
            SourceTeam = sourceTeam;
            HitPoint = hitPoint;
            HitDirection = hitDirection;
            KnockbackImpulse = knockbackImpulse;
        }

        public float Amount { get; }
        public CombatTeam SourceTeam { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitDirection { get; }
        public float KnockbackImpulse { get; }
    }
}
