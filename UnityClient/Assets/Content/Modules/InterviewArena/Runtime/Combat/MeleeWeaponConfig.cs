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
        [Tooltip("Full strike cycle: matches melee clip length × animator exit time.")]
        [SerializeField] private float strikeDuration = 2.16f;
        [SerializeField] [Range(0f, 1f)] private float hitWindowStartNormalized = 0.42f;
        [SerializeField] [Range(0f, 1f)] private float hitWindowEndNormalized = 0.54f;
        [SerializeField] private float forwardOffset = 0.85f;
        [SerializeField] private float strikeRadius = 0.95f;
        [SerializeField] private float knockbackImpulse = 4.5f;
        [SerializeField] private float strikeLungeImpulse = 2.8f;
        [SerializeField] private LayerMask hitMask = ~0;

        public float Damage => damage;
        public float StrikeDuration => Mathf.Max(0.05f, strikeDuration);
        public float Cooldown => StrikeDuration;
        public float HitWindowStartNormalized => Mathf.Clamp01(hitWindowStartNormalized);
        public float HitWindowEndNormalized => Mathf.Max(HitWindowStartNormalized, Mathf.Clamp01(hitWindowEndNormalized));
        public float ForwardOffset => forwardOffset;
        public float StrikeRadius => strikeRadius;
        public float KnockbackImpulse => knockbackImpulse;
        public float StrikeLungeImpulse => strikeLungeImpulse;
        public LayerMask HitMask => hitMask;
    }
}
