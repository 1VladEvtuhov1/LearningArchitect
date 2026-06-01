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

            if (disableColliders == null || disableColliders.Length == 0)
                disableColliders = GetComponentsInChildren<Collider>();

            if (disableRenderers == null || disableRenderers.Length == 0)
                disableRenderers = GetComponentsInChildren<Renderer>();

            CaptureSpawnPose();
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
            float delay = config != null ? config.RespawnDelay : 4.5f;
            SetAliveState(false);
            respawnTimer = delay;
        }

        private void Respawn()
        {
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            health.RestoreFullHealth();
            motor.Initialize(spawnPosition);
            SetAliveState(true);
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

            for (int i = 0; i < disableRenderers.Length; i++)
            {
                if (disableRenderers[i] != null)
                    disableRenderers[i].enabled = isAlive;
            }

            if (!isAlive && motor != null)
                motor.StopPlanarMotion();
        }
    }
}
