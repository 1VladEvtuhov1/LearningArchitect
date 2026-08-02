using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Applies hitstun action lock when the player receives damage.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(PlayerActionCoordinator))]
    public sealed class PlayerHealthHitstun : MonoBehaviour
    {
        [SerializeField] private PlayerConfig config;

        private Health health;
        private PlayerActionCoordinator actionCoordinator;
        private PlayerMotor playerMotor;
        private MeleeStrikeController meleeStrike;

        private void Awake()
        {
            health = GetComponent<Health>();
            actionCoordinator = GetComponent<PlayerActionCoordinator>();
            playerMotor = GetComponent<PlayerMotor>();
            meleeStrike = GetComponent<MeleeStrikeController>();
        }

        private void OnEnable()
        {
            if (health != null)
                health.Damaged += HandleDamaged;
        }

        private void OnDisable()
        {
            if (health != null)
                health.Damaged -= HandleDamaged;
        }

        public void ApplyConfig(PlayerConfig playerConfig) => config = playerConfig;

        private void HandleDamaged(Health _, DamageInfo __)
        {
            if (config == null || actionCoordinator == null)
                return;

            actionCoordinator.ApplyHitstun(config.HitstunDuration);
            if (playerMotor != null)
                playerMotor.InterruptDash();
            if (meleeStrike != null)
                meleeStrike.InterruptStrike();
        }
    }
}
