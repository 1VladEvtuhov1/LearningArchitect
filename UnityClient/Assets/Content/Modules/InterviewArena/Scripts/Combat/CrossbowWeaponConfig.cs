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
        [Tooltip("Cooldown after a shot/combo finishes before a new chain can start.")]
        [SerializeField] private float cooldown = 0.35f;
        [SerializeField] private float boltSpeed = 26f;
        [SerializeField] private float boltLifetime = 2.4f;
        [SerializeField] private float boltRadius = 0.12f;
        [SerializeField] private float knockbackImpulse = 3.2f;
        [SerializeField] private LayerMask hitMask = ~0;

        [Header("Shots / Combo")]
        [SerializeField] private CrossbowShotDefinition[] shots =
        {
            new CrossbowShotDefinition("Shot1", 0.55f, 0.35f, 0.18f, 0.40f, 0.40f, 1),
            new CrossbowShotDefinition("Shot2", 0.55f, 0.35f),
        };

        public float Damage => damage;
        public float Cooldown => Mathf.Max(0f, cooldown);
        public float BoltSpeed => boltSpeed;
        public float BoltLifetime => boltLifetime;
        public float BoltRadius => boltRadius;
        public float KnockbackImpulse => knockbackImpulse;
        public LayerMask HitMask => hitMask;
        public int ShotCount => shots != null ? shots.Length : 0;

        public bool TryGetShot(int index, out CrossbowShotDefinition shot)
        {
            if (shots == null || index < 0 || index >= shots.Length)
            {
                shot = null;
                return false;
            }

            shot = shots[index];
            return shot != null;
        }
    }
}
