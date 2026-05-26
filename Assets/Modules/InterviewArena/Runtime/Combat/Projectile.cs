using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    public sealed class Projectile : MonoBehaviour
    {
        private ProjectilePool ownerPool;
        private CrossbowWeaponConfig config;
        private CombatTeam sourceTeam;
        private int ownerInstanceId;
        private Vector3 velocity;
        private float lifetime;
        private readonly PhysicsQueryService queryService = new PhysicsQueryService();
        private bool isActive;

        public bool IsActive => isActive;

        public void Initialize(ProjectilePool pool, CrossbowWeaponConfig weaponConfig, CombatTeam team, int ownerId)
        {
            ownerPool = pool;
            config = weaponConfig;
            sourceTeam = team;
            ownerInstanceId = ownerId;
        }

        public void RefreshOwner(int ownerId)
        {
            ownerInstanceId = ownerId;
        }

        public void Launch(Vector3 position, Vector3 direction, CrossbowWeaponConfig runtimeConfig = null)
        {
            CrossbowWeaponConfig activeConfig = runtimeConfig != null ? runtimeConfig : config;
            if (activeConfig == null)
                return;

            config = activeConfig;
            Vector3 launchDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward;
            transform.position = position + launchDirection * Mathf.Max(0.05f, activeConfig.BoltRadius);
            velocity = launchDirection * activeConfig.BoltSpeed;
            lifetime = config.BoltLifetime;
            isActive = true;
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            isActive = false;
            velocity = Vector3.zero;
            lifetime = 0f;
            gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            if (!isActive || config == null)
                return;

            float stepDistance = velocity.magnitude * Time.fixedDeltaTime;
            if (stepDistance <= 0.0001f)
            {
                Expire();
                return;
            }

            Vector3 direction = velocity.normalized;
            if (queryService.TrySphereCastAlong(
                direction,
                transform.position,
                config.BoltRadius,
                stepDistance,
                config.HitMask,
                ownerInstanceId,
                out RaycastHit hit))
            {
                ResolveHit(hit);
                return;
            }

            transform.position += velocity * Time.fixedDeltaTime;
            lifetime -= Time.fixedDeltaTime;
            if (lifetime <= 0f)
                Expire();
        }

        private void ResolveHit(RaycastHit hit)
        {
            if (hit.collider != null
                && hit.collider.gameObject.GetInstanceID() != ownerInstanceId
                && CombatHitResolver.TryResolveDamageable(hit.collider, out IDamageable damageable))
            {
                DamageInfo info = new DamageInfo(
                    config.Damage,
                    sourceTeam,
                    hit.point,
                    velocity.normalized,
                    config.KnockbackImpulse);
                damageable.ApplyDamage(in info);
            }

            ownerPool?.Release(this);
        }

        private void Expire()
        {
            ownerPool?.Release(this);
        }
    }
}
