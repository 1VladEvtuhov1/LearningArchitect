using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [CreateAssetMenu(
        menuName = "Learning Architect/Interview Arena/Crossbow Weapon Config",
        fileName = "InterviewArena_CrossbowWeapon")]
    public sealed class CrossbowWeaponConfig : ScriptableObject
    {
        [Header("Bolt")]
        [SerializeField] private float damage = 22f;
        [SerializeField] private float cooldown = 0.9f;
        [SerializeField] private float boltSpeed = 26f;
        [SerializeField] private float boltLifetime = 2.4f;
        [SerializeField] private float boltRadius = 0.12f;
        [SerializeField] private float knockbackImpulse = 3.2f;
        [SerializeField] private LayerMask hitMask = ~0;

        public float Damage => damage;
        public float Cooldown => cooldown;
        public float BoltSpeed => boltSpeed;
        public float BoltLifetime => boltLifetime;
        public float BoltRadius => boltRadius;
        public float KnockbackImpulse => knockbackImpulse;
        public LayerMask HitMask => hitMask;
    }
}
