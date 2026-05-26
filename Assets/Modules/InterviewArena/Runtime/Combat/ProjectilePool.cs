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

        public void SetProjectilesRoot(Transform root)
        {
            projectilesRoot = root;
        }

        public void ApplyConfig(GameObject prefab, int capacity, CrossbowWeaponConfig weaponConfig, CombatTeam team, int ownerInstanceId)
        {
            boltPrefab = prefab;
            poolSize = Mathf.Max(4, capacity);
            BuildPool(weaponConfig, team, ownerInstanceId);
        }

        public void EnsureInitialized(CrossbowWeaponConfig weaponConfig, CombatTeam team, int ownerInstanceId)
        {
            if (boltPrefab == null)
            {
                projectiles = System.Array.Empty<Projectile>();
                return;
            }

            CrossbowWeaponConfig resolvedConfig = weaponConfig != null ? weaponConfig : ResolveWeaponConfig();
            if (resolvedConfig == null)
                return;

            if (projectiles == null || projectiles.Length == 0)
            {
                BuildPool(resolvedConfig, team, ownerInstanceId);
                return;
            }

            for (int i = 0; i < projectiles.Length; i++)
                projectiles[i]?.Initialize(this, resolvedConfig, team, ownerInstanceId);
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

            EnsureInitialized(weaponConfig, ResolveTeam(), ResolveOwnerInstanceId());

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

        private void Start()
        {
            EnsureInitialized(ResolveWeaponConfig(), ResolveTeam(), ResolveOwnerInstanceId());
            RefreshProjectileOwners();
        }

        private void RefreshProjectileOwners()
        {
            if (projectiles == null)
                return;

            int ownerId = ResolveOwnerInstanceId();
            for (int i = 0; i < projectiles.Length; i++)
                projectiles[i]?.RefreshOwner(ownerId);
        }

        private CrossbowWeaponConfig ResolveWeaponConfig()
        {
            CrossbowWeaponController crossbow = GetComponentInParent<CrossbowWeaponController>();
            return crossbow != null ? crossbow.Config : null;
        }

        private CombatTeam ResolveTeam()
        {
            CrossbowWeaponController crossbow = GetComponentInParent<CrossbowWeaponController>();
            return crossbow != null ? crossbow.SourceTeam : CombatTeam.Player;
        }

        private int ResolveOwnerInstanceId()
        {
            return transform.root.gameObject.GetInstanceID();
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
