using System;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// One authored crossbow shot: duration, movement lock, and optional combo chain timings.
    /// </summary>
    [Serializable]
    public sealed class CrossbowShotDefinition : IComboChainStep
    {
        public const int NoNextShot = -1;

        [SerializeField] private string id = "Shot";
        [SerializeField] private float shotDuration = 0.55f;
        [Tooltip("Planar locomotion lock at shot start.")]
        [SerializeField] private float movementLockDuration = 0.35f;

        [Header("Combo (seconds from shot start)")]
        [SerializeField] private float comboInputStart;
        [SerializeField] private float comboInputEnd;
        [SerializeField] private float comboTransitionTime;
        [Tooltip("Index of next shot in CrossbowWeaponConfig.shots. -1 = end of chain.")]
        [SerializeField] private int nextStrikeIndex = NoNextShot;

        public CrossbowShotDefinition()
        {
        }

        public CrossbowShotDefinition(
            string id,
            float shotDuration,
            float movementLockDuration,
            float comboInputStart = 0f,
            float comboInputEnd = 0f,
            float comboTransitionTime = 0f,
            int nextStrikeIndex = NoNextShot)
        {
            this.id = id;
            this.shotDuration = shotDuration;
            this.movementLockDuration = movementLockDuration;
            this.comboInputStart = comboInputStart;
            this.comboInputEnd = comboInputEnd;
            this.comboTransitionTime = comboTransitionTime;
            this.nextStrikeIndex = nextStrikeIndex;
        }

        public string Id => string.IsNullOrWhiteSpace(id) ? "Shot" : id;
        public float ShotDuration => Mathf.Max(0.05f, shotDuration);
        public float MovementLockDuration => Mathf.Clamp(movementLockDuration, 0f, ShotDuration);
        public float ComboInputStart => comboInputStart;
        public float ComboInputEnd => comboInputEnd;
        public float ComboTransitionTime => comboTransitionTime;
        public int NextStrikeIndex => nextStrikeIndex;

        public bool HasValidComboTimings
        {
            get
            {
                if (nextStrikeIndex < 0)
                    return true;

                float duration = ShotDuration;
                return comboInputStart >= 0f
                    && comboInputStart < comboInputEnd
                    && comboInputEnd <= comboTransitionTime
                    && comboTransitionTime <= duration;
            }
        }
    }
}
