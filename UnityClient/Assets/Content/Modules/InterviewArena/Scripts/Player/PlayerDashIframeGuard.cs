using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Absorbs normal hits during the middle dash i-frame window. No block VFX, no hitstun.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMotor))]
    public sealed class PlayerDashIframeGuard : MonoBehaviour, IIncomingHitGuard
    {
        private PlayerMotor playerMotor;

        private void Awake() => playerMotor = GetComponent<PlayerMotor>();

        public bool TryAbsorb(in DamageInfo incoming, out DamageInfo absorbed)
        {
            absorbed = incoming;
            if (playerMotor == null || !playerMotor.IsDashIframeActive)
                return false;

            absorbed = new DamageInfo(
                0f,
                incoming.SourceTeam,
                incoming.HitPoint,
                incoming.HitDirection,
                0f,
                wasBlocked: false);
            return true;
        }
    }
}
