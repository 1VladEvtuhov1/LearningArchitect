using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerCombatStance : MonoBehaviour
    {
        [SerializeField] private ArenaCombatStance defaultStance = ArenaCombatStance.BowAim;

        private PlayerInputReader input;
        private ArenaCombatStance currentStance;

        public ArenaCombatStance CurrentStance => currentStance;
        public bool IsBowAimStance => currentStance == ArenaCombatStance.BowAim;

        private void Awake()
        {
            input = GetComponent<PlayerInputReader>();
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
        }

        public void ToggleStance()
        {
            currentStance = currentStance == ArenaCombatStance.BowAim
                ? ArenaCombatStance.MeleeReady
                : ArenaCombatStance.BowAim;
        }
    }
}
