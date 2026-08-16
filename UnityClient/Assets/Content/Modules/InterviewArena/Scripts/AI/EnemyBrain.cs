using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemySensor))]
    [RequireComponent(typeof(EnemyMotor))]
    [RequireComponent(typeof(EnemyMeleeAttack))]
    [RequireComponent(typeof(EnemyCrossbowAttack))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(EnemyRespawnController))]
    [RequireComponent(typeof(EnemyHealthHitstun))]
    public sealed class EnemyBrain : MonoBehaviour
    {
        [SerializeField] private EnemyConfig config;
        [SerializeField] private Transform facingPivot;

        private EnemySensor sensor;
        private EnemyMotor motor;
        private EnemyMeleeAttack meleeAttack;
        private EnemyCrossbowAttack crossbowAttack;
        private Health health;
        private EnemyHealthHitstun hitstun;
        private EnemyState state = EnemyState.Patrol;

        public EnemyState State => state;

        public void ApplyConfig(EnemyConfig enemyConfig, Transform pivot)
        {
            config = enemyConfig;
            facingPivot = pivot;
            if (health != null && config != null)
            {
                health.ApplyConfig(
                    CombatTeam.Enemy,
                    config.MaxHealth,
                    config.InvulnerabilityDuration);
            }

            if (hitstun != null)
                hitstun.ApplyConfig(config);
        }

        public void BindPlayer(Transform player)
        {
            if (sensor != null)
                sensor.BindPlayer(player);
        }

        private void Awake()
        {
            sensor = GetComponent<EnemySensor>();
            motor = GetComponent<EnemyMotor>();
            meleeAttack = GetComponent<EnemyMeleeAttack>();
            crossbowAttack = GetComponent<EnemyCrossbowAttack>();
            health = GetComponent<Health>();
            hitstun = GetComponent<EnemyHealthHitstun>();

            if (facingPivot == null)
                facingPivot = transform;

            motor.Initialize(transform.position);
            if (config != null)
                ApplyConfig(config, facingPivot);
        }

        private void Update()
        {
            if (config == null || !health.IsAlive)
                return;

            float distance = float.MaxValue;
            float forwardDot = -1f;
            bool canSeePlayer = sensor.TrySamplePlayer(config, out distance, out forwardDot);
            bool shouldLose = EnemyFsmLogic.ShouldLosePlayer(distance, config.LoseDetectRange);
            bool inMeleeRange = sensor.IsInAttackRange(config, out _);
            bool inCrossbowRange = sensor.IsInCrossbowRange(config, out _);
            bool stunned = hitstun != null && hitstun.IsStunned;
            bool attackBusy = meleeAttack.IsBusy || stunned;
            bool rangedBusy = crossbowAttack.IsBusy || stunned;

            state = EnemyFsmLogic.ResolveNextState(
                state,
                canSeePlayer,
                shouldLose,
                inMeleeRange,
                inCrossbowRange,
                attackBusy,
                rangedBusy);

            meleeAttack.Tick(config, gameObject.GetInstanceID());
            if (stunned)
            {
                motor.StopPlanarMotion();
                return;
            }

            TickState(distance, attackBusy, rangedBusy);
        }

        private void TickState(float distanceToPlayer, bool attackBusy, bool rangedBusy)
        {
            switch (state)
            {
                case EnemyState.Patrol:
                    motor.TickPatrol(config, Time.deltaTime);
                    break;

                case EnemyState.Chase:
                    motor.TickChase(config, sensor.PlayerTarget, Time.deltaTime);
                    break;

                case EnemyState.Attack:
                    motor.StopPlanarMotion();
                    motor.FaceTarget(sensor.PlayerTarget, config, Time.deltaTime);
                    if (!attackBusy && distanceToPlayer <= config.AttackRange)
                        meleeAttack.TryBeginAttack(config);
                    break;

                case EnemyState.Ranged:
                    motor.StopPlanarMotion();
                    motor.FaceTarget(sensor.PlayerTarget, config, Time.deltaTime);
                    if (!rangedBusy && sensor.PlayerTarget != null)
                        crossbowAttack.TryFireAt(sensor.PlayerTarget);
                    break;
            }
        }
    }
}
