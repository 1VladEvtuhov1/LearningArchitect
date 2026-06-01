using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private MeleeStrikeController melee;
        [SerializeField] private CrossbowWeaponController crossbow;

        private PlayerInputReader input;
        private bool warnedMissingMelee;
        private bool warnedMissingCrossbow;

        private void Awake()
        {
            input = GetComponent<PlayerInputReader>();
            if (input == null)
                InterviewArenaAuthoringLog.MissingReference(this, nameof(input));
            if (melee == null)
                InterviewArenaAuthoringLog.MissingReference(this, nameof(melee));
            if (crossbow == null)
                InterviewArenaAuthoringLog.MissingReference(this, nameof(crossbow));
        }

        private void LateUpdate()
        {
            if (input == null)
                return;

            if (input.ConsumeMeleeAttack())
            {
                if (melee != null)
                    melee.TryStrike();
                else if (!warnedMissingMelee)
                {
                    warnedMissingMelee = true;
                    Debug.LogWarning("[InterviewArena] PlayerCombat: MeleeStrikeController reference is missing.", this);
                }
            }

            if (input.ConsumeCrossbowAttack())
            {
                if (crossbow != null)
                    crossbow.TryFire();
                else if (!warnedMissingCrossbow)
                {
                    warnedMissingCrossbow = true;
                    Debug.LogWarning("[InterviewArena] PlayerCombat: CrossbowWeaponController reference is missing.", this);
                }
            }
        }
    }
}
