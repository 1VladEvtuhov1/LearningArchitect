using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Scene-owned combat infrastructure (pooled projectiles, hit feedback). Not part of actor prefabs.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InterviewArenaCombatServices : MonoBehaviour
    {
        [SerializeField] private CrossbowWeaponConfig playerCrossbowConfig;
        [SerializeField] private ProjectilePool playerCrossbowPool;
        [SerializeField] private CombatHitFeedback combatFeedback;

        public CrossbowWeaponConfig PlayerCrossbowConfig => playerCrossbowConfig;
        public ProjectilePool PlayerCrossbowPool => playerCrossbowPool;
        public CombatHitFeedback CombatFeedback => combatFeedback;

        public void ConfigurePlayerProjectilePool(Transform projectilesRoot)
        {
            if (playerCrossbowPool == null)
                return;

            playerCrossbowPool.SetProjectilesRoot(projectilesRoot);
        }

        public void WirePlayerCrossbow(PlayerMotor player)
        {
            if (player == null)
            {
                InterviewArenaAuthoringLog.MissingReference(this, "player");
                return;
            }

            if (playerCrossbowPool == null)
            {
                InterviewArenaAuthoringLog.MissingReference(this, nameof(playerCrossbowPool));
                return;
            }

            if (playerCrossbowConfig == null)
            {
                InterviewArenaAuthoringLog.MissingReference(this, nameof(playerCrossbowConfig));
                return;
            }

            CrossbowWeaponController crossbow = player.GetComponent<CrossbowWeaponController>();
            if (crossbow == null)
            {
                InterviewArenaAuthoringLog.MissingReference(player, "CrossbowWeaponController");
                return;
            }

            if (crossbow.Muzzle == null)
            {
                InterviewArenaAuthoringLog.MissingReference(crossbow, "muzzle");
                return;
            }

            crossbow.ApplyConfig(
                playerCrossbowConfig,
                crossbow.Muzzle,
                playerCrossbowPool,
                CombatTeam.Player);
        }
    }
}
