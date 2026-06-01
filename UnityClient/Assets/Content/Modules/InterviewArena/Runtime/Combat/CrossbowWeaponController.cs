using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    public sealed class CrossbowWeaponController : MonoBehaviour
    {
        [SerializeField] private CrossbowWeaponConfig config;
        [SerializeField] private Transform muzzle;
        [SerializeField] private ProjectilePool projectilePool;
        [SerializeField] private CombatTeam sourceTeam = CombatTeam.Player;

        private float cooldownTimer;

        public CrossbowWeaponConfig Config => config;
        public CombatTeam SourceTeam => sourceTeam;
        public Transform Muzzle => muzzle;

        public void ApplyConfig(
            CrossbowWeaponConfig weaponConfig,
            Transform fireOrigin,
            ProjectilePool pool,
            CombatTeam team)
        {
            config = weaponConfig;
            muzzle = fireOrigin;
            projectilePool = pool;
            sourceTeam = team;
        }

        public bool TryFire()
        {
            if (config == null || projectilePool == null || cooldownTimer > 0f)
                return false;

            projectilePool.EnsureInitialized(
                config,
                sourceTeam,
                transform.root.gameObject.GetInstanceID());

            Transform origin = muzzle != null ? muzzle : transform;
            Vector3 direction = origin.forward.sqrMagnitude > 0.0001f
                ? origin.forward
                : transform.forward;

            if (!projectilePool.TryLaunch(origin.position, direction, config, out _))
                return false;

            CombatHitFeedback.PlayCrossbowFire(origin);
            cooldownTimer = config.Cooldown;
            return true;
        }

        private void Update()
        {
            cooldownTimer = Mathf.Max(0f, cooldownTimer - Time.deltaTime);
        }
    }
}
