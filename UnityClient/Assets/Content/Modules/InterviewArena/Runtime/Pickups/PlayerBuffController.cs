using System;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Tracks timed stat multipliers applied from world pickups.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerBuffController : MonoBehaviour
    {
        private const int MaxActiveBuffs = 4;

        [SerializeField] private ActiveBuff[] activeBuffs = new ActiveBuff[MaxActiveBuffs];

        public event Action Changed;

        public float GetMultiplier(BuffKind kind)
        {
            float multiplier = 1f;
            for (int i = 0; i < activeBuffs.Length; i++)
            {
                ActiveBuff buff = activeBuffs[i];
                if (buff.Kind != kind || buff.Remaining <= 0f)
                    continue;

                multiplier = Mathf.Max(multiplier, buff.Multiplier);
            }

            return multiplier;
        }

        public float GetRemaining(BuffKind kind)
        {
            float remaining = 0f;
            for (int i = 0; i < activeBuffs.Length; i++)
            {
                ActiveBuff buff = activeBuffs[i];
                if (buff.Kind != kind || buff.Remaining <= 0f)
                    continue;

                remaining = Mathf.Max(remaining, buff.Remaining);
            }

            return remaining;
        }

        public void ApplyBuff(BuffPickupConfig config)
        {
            if (config == null)
                return;

            int slot = FindSlot(config.Kind);
            if (slot < 0)
            {
                InterviewArenaAuthoringLog.MissingReference(this, "activeBuff slot capacity");
                return;
            }

            ActiveBuff buff = activeBuffs[slot];
            buff.Kind = config.Kind;
            buff.Multiplier = Mathf.Max(buff.Multiplier, config.Multiplier);
            buff.Remaining = config.Duration;
            activeBuffs[slot] = buff;

            Changed?.Invoke();
            CombatHitFeedback.PlayBuffApplied(transform.position, config.WorldColor);
        }

        private void Update()
        {
            bool changed = false;
            for (int i = 0; i < activeBuffs.Length; i++)
            {
                ActiveBuff buff = activeBuffs[i];
                if (buff.Remaining <= 0f)
                    continue;

                buff.Remaining -= Time.deltaTime;
                if (buff.Remaining <= 0f)
                {
                    buff.Remaining = 0f;
                    buff.Multiplier = 1f;
                    changed = true;
                }

                activeBuffs[i] = buff;
            }

            if (changed)
                Changed?.Invoke();
        }

        private int FindSlot(BuffKind kind)
        {
            int empty = -1;
            for (int i = 0; i < activeBuffs.Length; i++)
            {
                if (activeBuffs[i].Kind == kind && activeBuffs[i].Remaining > 0f)
                    return i;

                if (empty < 0 && activeBuffs[i].Remaining <= 0f)
                    empty = i;
            }

            return empty;
        }

        [Serializable]
        private struct ActiveBuff
        {
            public BuffKind Kind;
            public float Multiplier;
            public float Remaining;
        }
    }
}
