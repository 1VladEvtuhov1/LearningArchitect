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
            if (player == null || playerCrossbowPool == null || playerCrossbowConfig == null)
                return;

            CrossbowWeaponController crossbow = player.GetComponent<CrossbowWeaponController>();
            if (crossbow == null)
                return;

            Transform muzzle = player.transform.Find("ViewPivot/CrossbowMuzzle");
            if (muzzle == null)
                muzzle = player.ViewPivot;

            crossbow.ApplyConfig(
                playerCrossbowConfig,
                muzzle,
                playerCrossbowPool,
                CombatTeam.Player);
        }
    }
}
