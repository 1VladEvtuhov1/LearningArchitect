using System;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// One authored melee strike: full cycle length, movement commitment, hit window, and feel.
    /// Combo timings are seconds from strike start; hit window stays normalized 0–1.
    /// </summary>
    [Serializable]
    public sealed class MeleeStrikeDefinition : IComboChainStep
    {
        public const int NoNextStrike = -1;

        [SerializeField] private string id = "Light";
        [Tooltip("Optional Animator trigger. Empty = HumanoidAnimationProfileSO melee trigger.")]
        [SerializeField] private string animTriggerOverride = string.Empty;
        [SerializeField] private float damage = 28f;
        [Tooltip("Full strike cycle: matches melee clip length / Animator Melee state speed.")]
        [SerializeField] private float strikeDuration = 1.08f;
        [SerializeField] [Range(0f, 1f)] private float hitWindowStartNormalized = 0.38f;
        [SerializeField] [Range(0f, 1f)] private float hitWindowEndNormalized = 0.52f;
        [SerializeField] private float forwardOffset = 0.85f;
        [SerializeField] private float strikeRadius = 0.95f;
        [SerializeField] private float knockbackImpulse = 4.5f;
        [Tooltip("Planar travel in meters during the step window. 0 = no body step.")]
        [SerializeField] private float strikeStepDistance;
        [Tooltip("Seconds from strike start when the body step begins.")]
        [SerializeField] private float strikeStepStart;
        [Tooltip("Seconds from strike start when the body step ends (exclusive).")]
        [SerializeField] private float strikeStepEnd;
        [Tooltip("WASD and body-turn lock from strike start. Equal to Strike Duration plants for the full swing.")]
        [SerializeField] private float movementLockDuration = 0.45f;

        [Header("Combo (seconds from strike start)")]
        [Tooltip("Start of half-open combo input window. Ignored when Next Strike Index is -1.")]
        [SerializeField] private float comboInputStart;
        [Tooltip("End of half-open combo input window (exclusive). Ignored when Next Strike Index is -1.")]
        [SerializeField] private float comboInputEnd;
        [Tooltip("When a queued next strike actually begins. Ignored when Next Strike Index is -1.")]
        [SerializeField] private float comboTransitionTime;
        [Tooltip("Index of next strike in MeleeWeaponConfig.strikes. -1 = end of chain.")]
        [SerializeField] private int nextStrikeIndex = NoNextStrike;

        public MeleeStrikeDefinition()
        {
        }

        public MeleeStrikeDefinition(
            string id,
            float strikeDuration,
            float movementLockDuration,
            float damage = 28f,
            float hitWindowStartNormalized = 0.38f,
            float hitWindowEndNormalized = 0.52f,
            float forwardOffset = 0.85f,
            float strikeRadius = 0.95f,
            float knockbackImpulse = 4.5f,
            float strikeStepDistance = 0f,
            float strikeStepStart = 0f,
            float strikeStepEnd = 0f,
            string animTriggerOverride = "",
            float comboInputStart = 0f,
            float comboInputEnd = 0f,
            float comboTransitionTime = 0f,
            int nextStrikeIndex = NoNextStrike)
        {
            this.id = id;
            this.strikeDuration = strikeDuration;
            this.movementLockDuration = movementLockDuration;
            this.damage = damage;
            this.hitWindowStartNormalized = hitWindowStartNormalized;
            this.hitWindowEndNormalized = hitWindowEndNormalized;
            this.forwardOffset = forwardOffset;
            this.strikeRadius = strikeRadius;
            this.knockbackImpulse = knockbackImpulse;
            this.strikeStepDistance = strikeStepDistance;
            this.strikeStepStart = strikeStepStart;
            this.strikeStepEnd = strikeStepEnd;
            this.animTriggerOverride = animTriggerOverride ?? string.Empty;
            this.comboInputStart = comboInputStart;
            this.comboInputEnd = comboInputEnd;
            this.comboTransitionTime = comboTransitionTime;
            this.nextStrikeIndex = nextStrikeIndex;
        }

        public string Id => string.IsNullOrWhiteSpace(id) ? "Strike" : id;
        public string AnimTriggerOverride => animTriggerOverride ?? string.Empty;
        public float Damage => damage;
        public float StrikeDuration => Mathf.Max(0.05f, strikeDuration);
        public float HitWindowStartNormalized => Mathf.Clamp01(hitWindowStartNormalized);
        public float HitWindowEndNormalized =>
            Mathf.Max(HitWindowStartNormalized, Mathf.Clamp01(hitWindowEndNormalized));
        public float ForwardOffset => forwardOffset;
        public float StrikeRadius => strikeRadius;
        public float KnockbackImpulse => knockbackImpulse;
        public float StrikeStepDistance => Mathf.Max(0f, strikeStepDistance);
        public float StrikeStepStart => Mathf.Max(0f, strikeStepStart);
        public float StrikeStepEnd => Mathf.Max(StrikeStepStart, strikeStepEnd);
        public float MovementLockDuration => Mathf.Clamp(movementLockDuration, 0f, StrikeDuration);
        public float ComboInputStart => comboInputStart;
        public float ComboInputEnd => comboInputEnd;
        public float ComboTransitionTime => comboTransitionTime;
        public int NextStrikeIndex => nextStrikeIndex;

        /// <summary>
        /// Combo timing chain is validated only when a next strike is authored.
        /// </summary>
        public bool HasValidComboTimings
        {
            get
            {
                if (nextStrikeIndex < 0)
                    return true;

                float duration = StrikeDuration;
                return comboInputStart >= 0f
                    && comboInputStart < comboInputEnd
                    && comboInputEnd <= comboTransitionTime
                    && comboTransitionTime <= duration;
            }
        }

        public static bool TryDescribeComboTimingError(
            MeleeStrikeDefinition strike,
            int strikeCount,
            out string error)
        {
            error = null;
            if (strike == null)
            {
                error = "Strike definition is null.";
                return true;
            }

            if (strike.nextStrikeIndex < 0)
                return false;

            if (strike.nextStrikeIndex >= strikeCount)
            {
                error =
                    $"Strike '{strike.Id}': nextStrikeIndex {strike.nextStrikeIndex} is out of range (count={strikeCount}).";
                return true;
            }

            if (!strike.HasValidComboTimings)
            {
                error =
                    $"Strike '{strike.Id}': require 0 <= comboInputStart < comboInputEnd <= comboTransitionTime <= strikeDuration " +
                    $"({strike.comboInputStart}, {strike.comboInputEnd}, {strike.comboTransitionTime}, {strike.StrikeDuration}).";
                return true;
            }

            return false;
        }

        public static bool TryDescribeStepTimingError(MeleeStrikeDefinition strike, out string error)
        {
            error = null;
            if (strike == null)
            {
                error = "Strike definition is null.";
                return true;
            }

            if (strike.StrikeStepDistance <= 0f)
                return false;

            float start = strike.StrikeStepStart;
            float end = strike.StrikeStepEnd;
            float duration = strike.StrikeDuration;
            if (start < end && end <= duration)
                return false;

            error =
                $"Strike '{strike.Id}': require 0 <= strikeStepStart < strikeStepEnd <= strikeDuration " +
                $"({start}, {end}, {duration}).";
            return true;
        }
    }

    /// <summary>
    /// Constant-speed planar attack step. Position stays with the motor, not Animator.
    /// Window is half-open: start &lt;= elapsed &lt; end.
    /// </summary>
    public static class MeleeStrikeStep
    {
        public static bool TryResolvePlanarVelocity(
            MeleeStrikeDefinition strike,
            float strikeElapsed,
            Vector3 planarDirection,
            out Vector3 planarVelocity)
        {
            planarVelocity = Vector3.zero;
            if (strike == null)
                return false;

            float distance = strike.StrikeStepDistance;
            if (distance <= 0f)
                return false;

            float start = strike.StrikeStepStart;
            float end = Mathf.Min(strike.StrikeStepEnd, strike.StrikeDuration);
            if (end <= start)
                return false;

            if (strikeElapsed < start || strikeElapsed >= end)
                return false;

            planarDirection.y = 0f;
            if (planarDirection.sqrMagnitude < 0.0001f)
                return false;

            float speed = distance / (end - start);
            planarVelocity = planarDirection.normalized * speed;
            return true;
        }
    }
}
