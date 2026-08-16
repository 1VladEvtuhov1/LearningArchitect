using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class EnemyRespawnController : MonoBehaviour
    {
        [SerializeField] private EnemyConfig config;
        [SerializeField] private EnemyBrain brain;
        [SerializeField] private EnemyMotor motor;
        [SerializeField] private EnemyMeleeAttack meleeAttack;
        [SerializeField] private EnemyCrossbowAttack crossbowAttack;
        [SerializeField] private Collider[] disableColliders;
        [SerializeField] private Renderer[] disableRenderers;

        private Health health;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private float respawnTimer = -1f;
        private bool[] rendererEnabledWhenAlive;
        private bool managedTargetsResolved;

        public void ApplyConfig(EnemyConfig enemyConfig)
        {
            config = enemyConfig;
        }

        public void CaptureSpawnPose()
        {
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        private void Awake()
        {
            health = GetComponent<Health>();
            brain = brain != null ? brain : GetComponent<EnemyBrain>();
            motor = motor != null ? motor : GetComponent<EnemyMotor>();
            meleeAttack = meleeAttack != null ? meleeAttack : GetComponent<EnemyMeleeAttack>();
            crossbowAttack = crossbowAttack != null ? crossbowAttack : GetComponent<EnemyCrossbowAttack>();
            CaptureSpawnPose();
        }

        private void Start()
        {
            // After ArenaHumanoidVisual has bound the nested visual actor in Awake.
            EnsureManagedTargets();
        }

        private void OnEnable()
        {
            if (health != null)
                health.Died += HandleDied;
        }

        private void OnDisable()
        {
            if (health != null)
                health.Died -= HandleDied;
        }

        private void Update()
        {
            if (respawnTimer < 0f)
                return;

            respawnTimer -= Time.deltaTime;
            if (respawnTimer > 0f)
                return;

            respawnTimer = -1f;
            Respawn();
        }

        private void HandleDied(Health _)
        {
            EnsureManagedTargets();
            float delay = config != null ? config.RespawnDelay : 4.5f;
            SetAliveState(false);
            respawnTimer = delay;
        }

        private void Respawn()
        {
            EnsureManagedTargets();
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            health.RestoreFullHealth();
            motor.Initialize(spawnPosition);
            SetAliveState(true);
        }

        private void EnsureManagedTargets()
        {
            if (managedTargetsResolved)
                return;

            if (disableColliders == null || disableColliders.Length == 0)
                disableColliders = GetComponentsInChildren<Collider>(true);

            if (disableRenderers == null || disableRenderers.Length == 0)
                disableRenderers = EnemyRespawnVisualRules.CollectManagedRenderers(transform);

            managedTargetsResolved = true;
        }

        private void SetAliveState(bool isAlive)
        {
            if (brain != null)
                brain.enabled = isAlive;

            if (motor != null)
                motor.enabled = isAlive;

            if (meleeAttack != null)
                meleeAttack.enabled = isAlive;

            if (crossbowAttack != null)
                crossbowAttack.enabled = isAlive;

            for (int i = 0; i < disableColliders.Length; i++)
            {
                if (disableColliders[i] != null)
                    disableColliders[i].enabled = isAlive;
            }

            if (!isAlive)
                CaptureAndDisableRenderers();
            else
                RestoreRenderers();

            if (!isAlive && motor != null)
                motor.StopPlanarMotion();
        }

        private void CaptureAndDisableRenderers()
        {
            if (disableRenderers == null)
                return;

            if (rendererEnabledWhenAlive == null || rendererEnabledWhenAlive.Length != disableRenderers.Length)
                rendererEnabledWhenAlive = new bool[disableRenderers.Length];

            for (int i = 0; i < disableRenderers.Length; i++)
            {
                Renderer renderer = disableRenderers[i];
                if (renderer == null)
                {
                    rendererEnabledWhenAlive[i] = false;
                    continue;
                }

                rendererEnabledWhenAlive[i] = renderer.enabled;
                renderer.enabled = false;
            }
        }

        private void RestoreRenderers()
        {
            if (disableRenderers == null)
                return;

            for (int i = 0; i < disableRenderers.Length; i++)
            {
                Renderer renderer = disableRenderers[i];
                if (renderer == null)
                    continue;

                bool wasEnabled = rendererEnabledWhenAlive != null
                    && i < rendererEnabledWhenAlive.Length
                    && rendererEnabledWhenAlive[i];
                renderer.enabled = wasEnabled;
            }
        }
    }
}
