using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [CreateAssetMenu(
        menuName = "Learning Architect/Interview Arena/Melee Weapon Config",
        fileName = "InterviewArena_MeleeWeapon")]
    public sealed class MeleeWeaponConfig : ScriptableObject
    {
        [Header("Strike")]
        [SerializeField] private float damage = 28f;
        [SerializeField] private float cooldown = 0.55f;
        [SerializeField] private float forwardOffset = 0.85f;
        [SerializeField] private float strikeRadius = 0.95f;
        [SerializeField] private float knockbackImpulse = 4.5f;
        [SerializeField] private float strikeLungeImpulse = 2.8f;
        [SerializeField] private LayerMask hitMask = ~0;

        public float Damage => damage;
        public float Cooldown => cooldown;
        public float ForwardOffset => forwardOffset;
        public float StrikeRadius => strikeRadius;
        public float KnockbackImpulse => knockbackImpulse;
        public float StrikeLungeImpulse => strikeLungeImpulse;
        public LayerMask HitMask => hitMask;
    }
}
