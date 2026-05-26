using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [CreateAssetMenu(
        menuName = "Learning Architect/Interview Arena/Enemy Config",
        fileName = "InterviewArena_EnemyConfig")]
    public sealed class EnemyConfig : ScriptableObject
    {
        [Header("Vitality")]
        [SerializeField] private float maxHealth = 90f;
        [SerializeField] private float invulnerabilityDuration = 0.35f;
        [SerializeField] private float respawnDelay = 4.5f;

        [Header("Awareness")]
        [SerializeField] private float detectRange = 9f;
        [SerializeField] private float loseDetectRange = 12f;
        [SerializeField] private float visibilityDotThreshold = -0.15f;

        [Header("Movement")]
        [SerializeField] private float patrolSpeed = 2.2f;
        [SerializeField] private float chaseSpeed = 4.6f;
        [SerializeField] private float patrolRadius = 3.5f;
        [SerializeField] private float patrolIdleDuration = 0.8f;
        [SerializeField] private float rotationSpeed = 10f;

        [Header("Attack")]
        [SerializeField] private float attackRange = 1.65f;
        [SerializeField] private float attackCooldown = 1.05f;
        [SerializeField] private float attackWindup = 0.22f;
        [SerializeField] private float attackDamage = 16f;
        [SerializeField] private float attackKnockback = 5.5f;
        [SerializeField] private float strikeForwardOffset = 0.75f;
        [SerializeField] private float strikeRadius = 0.9f;
        [SerializeField] private LayerMask hitMask = ~0;

        [Header("Crossbow")]
        [SerializeField] private CrossbowWeaponConfig crossbowConfig;
        [SerializeField] private float crossbowMinRange = 2.2f;
        [SerializeField] private float crossbowMaxRange = 11f;
        [SerializeField] private int crossbowPoolSize = 12;

        public float MaxHealth => maxHealth;
        public float InvulnerabilityDuration => invulnerabilityDuration;
        public float RespawnDelay => respawnDelay;
        public float DetectRange => detectRange;
        public float LoseDetectRange => loseDetectRange;
        public float VisibilityDotThreshold => visibilityDotThreshold;
        public float PatrolSpeed => patrolSpeed;
        public float ChaseSpeed => chaseSpeed;
        public float PatrolRadius => patrolRadius;
        public float PatrolIdleDuration => patrolIdleDuration;
        public float RotationSpeed => rotationSpeed;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public float AttackWindup => attackWindup;
        public float AttackDamage => attackDamage;
        public float AttackKnockback => attackKnockback;
        public float StrikeForwardOffset => strikeForwardOffset;
        public float StrikeRadius => strikeRadius;
        public LayerMask HitMask => hitMask;
        public CrossbowWeaponConfig CrossbowConfig => crossbowConfig;
        public float CrossbowMinRange => crossbowMinRange;
        public float CrossbowMaxRange => crossbowMaxRange;
        public int CrossbowPoolSize => crossbowPoolSize;
    }
}
