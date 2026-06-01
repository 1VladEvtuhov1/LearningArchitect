using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    public sealed class ProjectilePool : MonoBehaviour
    {
        [SerializeField] private GameObject boltPrefab;
        [SerializeField] private int poolSize = 24;
        [SerializeField] private Transform projectilesRoot;

        private Projectile[] projectiles;
        private CrossbowWeaponConfig cachedWeaponConfig;
        private CombatTeam cachedTeam = CombatTeam.Player;
        private int cachedOwnerInstanceId;

        public void SetProjectilesRoot(Transform root)
        {
            projectilesRoot = root;
        }

        public void ApplyConfig(GameObject prefab, int capacity, CrossbowWeaponConfig weaponConfig, CombatTeam team, int ownerInstanceId)
        {
            boltPrefab = prefab;
            poolSize = Mathf.Max(4, capacity);
            cachedWeaponConfig = weaponConfig;
            cachedTeam = team;
            cachedOwnerInstanceId = ownerInstanceId;
            BuildPool(weaponConfig, team, ownerInstanceId);
        }

        public void EnsureInitialized(CrossbowWeaponConfig weaponConfig, CombatTeam team, int ownerInstanceId)
        {
            if (boltPrefab == null)
            {
                projectiles = System.Array.Empty<Projectile>();
                return;
            }

            CrossbowWeaponConfig resolvedConfig = weaponConfig != null ? weaponConfig : cachedWeaponConfig;
            if (resolvedConfig == null)
            {
                InterviewArenaAuthoringLog.MissingReference(this, nameof(cachedWeaponConfig));
                return;
            }

            CombatTeam resolvedTeam = team;
            int resolvedOwnerId = ownerInstanceId;

            if (projectiles == null || projectiles.Length == 0)
            {
                BuildPool(resolvedConfig, resolvedTeam, resolvedOwnerId);
                return;
            }

            for (int i = 0; i < projectiles.Length; i++)
                projectiles[i]?.Initialize(this, resolvedConfig, resolvedTeam, resolvedOwnerId);
        }

        public bool TryLaunch(
            Vector3 position,
            Vector3 direction,
            CrossbowWeaponConfig weaponConfig,
            out Projectile projectile)
        {
            projectile = null;
            if (weaponConfig == null || direction.sqrMagnitude < 0.0001f)
                return false;

            EnsureInitialized(weaponConfig, cachedTeam, cachedOwnerInstanceId);

            if (projectiles == null || projectiles.Length == 0)
                return false;

            Vector3 launchDirection = direction.normalized;
            for (int i = 0; i < projectiles.Length; i++)
            {
                Projectile candidate = projectiles[i];
                if (candidate == null || candidate.IsActive)
                    continue;

                candidate.Launch(position, launchDirection, weaponConfig);
                projectile = candidate;
                return true;
            }

            return false;
        }

        public void Release(Projectile projectile)
        {
            if (projectile == null)
                return;

            projectile.Deactivate();
        }

        private void BuildPool(CrossbowWeaponConfig weaponConfig, CombatTeam team, int ownerInstanceId)
        {
            if (boltPrefab == null || weaponConfig == null)
            {
                projectiles = System.Array.Empty<Projectile>();
                return;
            }

            if (projectiles != null)
            {
                for (int i = 0; i < projectiles.Length; i++)
                {
                    if (projectiles[i] != null)
                        Destroy(projectiles[i].gameObject);
                }
            }

            Transform parent = projectilesRoot != null ? projectilesRoot : transform;
            projectiles = new Projectile[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                GameObject instance = Instantiate(boltPrefab, parent);
                instance.name = "CrossbowBolt_" + i;
                instance.SetActive(false);

                Projectile projectile = instance.GetComponent<Projectile>();
                if (projectile == null)
                    projectile = instance.AddComponent<Projectile>();

                projectile.Initialize(this, weaponConfig, team, ownerInstanceId);
                projectiles[i] = projectile;
            }
        }
    }
}
