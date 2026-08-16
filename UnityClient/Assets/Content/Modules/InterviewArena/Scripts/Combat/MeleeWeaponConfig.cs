using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [CreateAssetMenu(
        menuName = "Learning Architect/Interview Arena/Melee Weapon Config",
        fileName = "InterviewArena_MeleeWeapon")]
    public sealed class MeleeWeaponConfig : ScriptableObject
    {
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private MeleeStrikeDefinition[] strikes =
        {
            new MeleeStrikeDefinition("Light1", strikeDuration: 0.80f, movementLockDuration: 0.28f),
        };

        public LayerMask HitMask => hitMask;
        public int StrikeCount => strikes != null ? strikes.Length : 0;

        public MeleeStrikeDefinition GetStrike(int index)
        {
            if (strikes == null || strikes.Length == 0)
                return new MeleeStrikeDefinition();

            int clamped = Mathf.Clamp(index, 0, strikes.Length - 1);
            MeleeStrikeDefinition strike = strikes[clamped];
            return strike ?? new MeleeStrikeDefinition();
        }

        public bool TryGetStrike(int index, out MeleeStrikeDefinition strike)
        {
            if (strikes == null || index < 0 || index >= strikes.Length || strikes[index] == null)
            {
                strike = null;
                return false;
            }

            strike = strikes[index];
            return true;
        }

        /// <summary>
        /// Replaces authored strikes (Edit Mode tests / tooling). Prefer Inspector for content.
        /// </summary>
        public void SetStrikes(params MeleeStrikeDefinition[] definitions)
        {
            strikes = definitions ?? System.Array.Empty<MeleeStrikeDefinition>();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (strikes == null || strikes.Length == 0)
            {
                strikes = new[] { new MeleeStrikeDefinition() };
                return;
            }

            int count = strikes.Length;
            for (int i = 0; i < count; i++)
            {
                MeleeStrikeDefinition strike = strikes[i];
                if (strike == null)
                    continue;

                if (MeleeStrikeDefinition.TryDescribeComboTimingError(strike, count, out string error))
                    Debug.LogWarning($"[MeleeWeaponConfig] {name}: {error}", this);

                if (MeleeStrikeDefinition.TryDescribeStepTimingError(strike, out string stepError))
                    Debug.LogWarning($"[MeleeWeaponConfig] {name}: {stepError}", this);
            }
        }
#endif
    }
}
