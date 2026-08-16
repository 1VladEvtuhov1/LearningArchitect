using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerCombatStance : MonoBehaviour
    {
        [SerializeField] private ArenaCombatStance defaultStance = ArenaCombatStance.MeleeReady;

        private PlayerInputReader input;
        private MeleeStrikeController meleeStrike;
        private CrossbowWeaponController crossbow;
        // Matches defaultStance so Edit Mode tests see MeleeReady before Awake runs.
        private ArenaCombatStance currentStance = ArenaCombatStance.MeleeReady;

        public ArenaCombatStance CurrentStance => currentStance;
        public bool IsBowAimStance => currentStance == ArenaCombatStance.BowAim;

        private void Awake()
        {
            input = GetComponent<PlayerInputReader>();
            meleeStrike = GetComponent<MeleeStrikeController>();
            crossbow = GetComponent<CrossbowWeaponController>();
            currentStance = defaultStance;
        }

        private void Update()
        {
            if (input != null && input.ConsumeStanceToggle())
                ToggleStance();
        }

        public void SetStance(ArenaCombatStance stance)
        {
            currentStance = stance;

            if (stance == ArenaCombatStance.BowAim)
            {
                if (meleeStrike == null)
                    meleeStrike = GetComponent<MeleeStrikeController>();

                if (meleeStrike != null && meleeStrike.IsStrikeActive)
                    meleeStrike.InterruptStrike();
                return;
            }

            if (crossbow == null)
                crossbow = GetComponent<CrossbowWeaponController>();

            if (crossbow != null && crossbow.IsShotActive)
                crossbow.InterruptShot();
        }

        public void ToggleStance()
        {
            SetStance(
                currentStance == ArenaCombatStance.BowAim
                    ? ArenaCombatStance.MeleeReady
                    : ArenaCombatStance.BowAim);
        }
    }
}
