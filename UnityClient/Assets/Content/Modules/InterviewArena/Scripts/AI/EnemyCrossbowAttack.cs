using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    public sealed class EnemyCrossbowAttack : MonoBehaviour
    {
        [SerializeField] private Transform aimOrigin;
        [SerializeField] private ProjectilePool projectilePool;
        [SerializeField] private CombatTeam sourceTeam = CombatTeam.Enemy;

        private CrossbowWeaponConfig config;
        private float cooldownTimer;
        private float shootAnimTimer;
        private int shootAnimStartVersion;

        public Transform AimOrigin => aimOrigin != null ? aimOrigin : transform;
        public bool IsBusy => cooldownTimer > 0f;
        public bool IsShootingAnimActive => shootAnimTimer > 0f;
        public int ShootAnimStartVersion => shootAnimStartVersion;

        public void ApplyConfig(CrossbowWeaponConfig weaponConfig, Transform origin, ProjectilePool pool, CombatTeam team)
        {
            config = weaponConfig;
            aimOrigin = origin;
            projectilePool = pool;
            sourceTeam = team;
        }

        public void InterruptShot()
        {
            shootAnimTimer = 0f;
        }

        public bool TryFireAt(Transform target)
        {
            if (config == null || projectilePool == null || target == null || cooldownTimer > 0f)
                return false;

            Vector3 direction = target.position - AimOrigin.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
                return false;

            direction.Normalize();
            AimOrigin.rotation = Quaternion.LookRotation(direction, Vector3.up);

            projectilePool.EnsureInitialized(
                config,
                sourceTeam,
                transform.root.gameObject.GetInstanceID());

            if (!projectilePool.TryLaunch(AimOrigin.position, direction, config, out _))
                return false;

            cooldownTimer = config.Cooldown;
            shootAnimTimer = 0.35f;
            shootAnimStartVersion++;
            return true;
        }

        private void Update()
        {
            cooldownTimer = Mathf.Max(0f, cooldownTimer - Time.deltaTime);
            if (shootAnimTimer > 0f)
                shootAnimTimer = Mathf.Max(0f, shootAnimTimer - Time.deltaTime);
        }
    }
}
