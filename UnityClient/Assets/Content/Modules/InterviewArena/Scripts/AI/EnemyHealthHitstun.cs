using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Cancels an in-flight enemy attack and locks new attacks while the hit react plays.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(EnemyMeleeAttack))]
    [RequireComponent(typeof(EnemyCrossbowAttack))]
    public sealed class EnemyHealthHitstun : MonoBehaviour
    {
        [SerializeField] private EnemyConfig config;

        private Health health;
        private EnemyMeleeAttack meleeAttack;
        private EnemyCrossbowAttack crossbowAttack;
        private float stunTimer;

        public bool IsStunned => stunTimer > 0f;

        private void Awake() => Bind();

        private void OnEnable()
        {
            Bind();
            if (health != null)
            {
                health.Damaged -= HandleDamaged;
                health.Damaged += HandleDamaged;
            }
        }

        private void OnDisable()
        {
            if (health != null)
                health.Damaged -= HandleDamaged;
        }

        public void ApplyConfig(EnemyConfig enemyConfig)
        {
            config = enemyConfig;
            Bind();
            if (isActiveAndEnabled && health != null)
            {
                health.Damaged -= HandleDamaged;
                health.Damaged += HandleDamaged;
            }
        }

        private void Bind()
        {
            if (health == null)
                health = GetComponent<Health>();
            if (meleeAttack == null)
                meleeAttack = GetComponent<EnemyMeleeAttack>();
            if (crossbowAttack == null)
                crossbowAttack = GetComponent<EnemyCrossbowAttack>();
        }

        public void Tick(float deltaTime)
        {
            if (stunTimer <= 0f)
                return;

            if (deltaTime < 0f)
                deltaTime = 0f;

            stunTimer = Mathf.Max(0f, stunTimer - deltaTime);
        }

        private void Update() => Tick(Time.deltaTime);

        private void HandleDamaged(Health _, DamageInfo __)
        {
            if (meleeAttack != null)
                meleeAttack.Interrupt();
            if (crossbowAttack != null)
                crossbowAttack.InterruptShot();

            stunTimer = config != null ? config.HitstunDuration : 0.85f;
        }
    }
}
