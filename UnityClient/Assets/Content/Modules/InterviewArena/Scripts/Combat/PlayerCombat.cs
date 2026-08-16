using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerActionCoordinator))]
    public sealed class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private MeleeStrikeController melee;
        [SerializeField] private CrossbowWeaponController crossbow;

        private PlayerInputReader input;
        private PlayerActionCoordinator actionCoordinator;
        private MeleeBlockController meleeBlock;
        private bool warnedMissingMelee;
        private bool warnedMissingCrossbow;

        private void Awake()
        {
            input = GetComponent<PlayerInputReader>();
            actionCoordinator = GetComponent<PlayerActionCoordinator>();
            meleeBlock = GetComponent<MeleeBlockController>();
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

            if (meleeBlock == null)
                meleeBlock = GetComponent<MeleeBlockController>();

            bool canAttack = actionCoordinator != null && actionCoordinator.CanAttack;
            bool strikeActive = melee != null && melee.IsStrikeActive;
            bool shotActive = crossbow != null && crossbow.IsShotActive;
            bool blocking = meleeBlock != null && meleeBlock.IsBlocking;

            // Strict combo semantics: any LMB during an active strike is a combo attempt and is
            // consumed even when rejected (early/late/final). Idle forgiveness unchanged:
            // consume only when CanAttack allows starting a new strike. Blocking: LMB cancels guard.
            if (PlayerCombatMeleeInputRules.ShouldConsumeMeleePress(strikeActive, canAttack, blocking))
            {
                if (input.ConsumeMeleeAttack())
                {
                    if (blocking)
                        meleeBlock.Stop();
                    HandleMeleeAttackConsumed();
                }
            }

            if (PlayerCombatCrossbowInputRules.ShouldConsumeCrossbowPress(shotActive, canAttack))
            {
                if (input.ConsumeCrossbowAttack())
                    HandleCrossbowAttackConsumed();
            }
        }

        private void HandleMeleeAttackConsumed()
        {
            if (TryGetComponent(out PlayerCombatStance stance))
                stance.SetStance(ArenaCombatStance.MeleeReady);

            if (melee == null)
            {
                if (!warnedMissingMelee)
                {
                    warnedMissingMelee = true;
                    Debug.LogWarning("[InterviewArena] PlayerCombat: MeleeStrikeController reference is missing.", this);
                }

                return;
            }

            if (crossbow != null && crossbow.IsShotActive)
                crossbow.InterruptShot();

            melee.HandleMeleeAttackInput();
        }

        private void HandleCrossbowAttackConsumed()
        {
            if (TryGetComponent(out PlayerCombatStance stance))
                stance.SetStance(ArenaCombatStance.BowAim);

            if (crossbow == null)
            {
                if (!warnedMissingCrossbow)
                {
                    warnedMissingCrossbow = true;
                    Debug.LogWarning("[InterviewArena] PlayerCombat: CrossbowWeaponController reference is missing.", this);
                }

                return;
            }

            crossbow.HandleFireInput();
        }
    }
}
