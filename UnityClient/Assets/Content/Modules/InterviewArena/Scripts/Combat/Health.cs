using System;
using System.Collections.Generic;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    public sealed class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private CombatTeam team = CombatTeam.Enemy;
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float invulnerabilityDuration = 0.45f;

        private float currentHealth;
        private InvulnerabilityTimer invulnerability;
        private readonly List<Component> incomingHitGuards = new List<Component>(4);

        public event Action<Health> Died;
        public event Action<Health, DamageInfo> Damaged;
        public event Action<Health, DamageInfo> Blocked;

        public CombatTeam Team => team;
        public bool IsAlive => currentHealth > 0f;
        public bool IsInvulnerable => invulnerability.IsActive;
        public float InvulnerabilityDuration => invulnerabilityDuration;
        public float InvulnerabilityRemaining => invulnerability.Remaining;
        public float InvulnerabilityNormalized =>
            invulnerabilityDuration > 0f && invulnerability.IsActive
                ? invulnerability.Remaining / invulnerabilityDuration
                : 0f;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void ApplyConfig(CombatTeam combatTeam, float healthCapacity, float iframeSeconds = -1f)
        {
            team = combatTeam;
            maxHealth = Mathf.Max(1f, healthCapacity);
            currentHealth = maxHealth;
            if (iframeSeconds >= 0f)
                invulnerabilityDuration = iframeSeconds;
            invulnerability.Clear();
        }

        public void ApplyDamage(in DamageInfo info)
        {
            if (!IsAlive)
                return;

            if (invulnerability.BlocksIncomingDamage)
                return;

            if (!CombatRules.CanDamage(info.SourceTeam, team))
                return;

            DamageInfo resolved = info;
            incomingHitGuards.Clear();
            GetComponents(typeof(IIncomingHitGuard), incomingHitGuards);
            for (int i = 0; i < incomingHitGuards.Count; i++)
            {
                if (incomingHitGuards[i] is IIncomingHitGuard guard
                    && guard.TryAbsorb(in info, out DamageInfo absorbed))
                {
                    resolved = absorbed;
                    break;
                }
            }

            if (resolved.WasBlocked)
            {
                Blocked?.Invoke(this, resolved);
                CombatHitFeedback.PlayBlock(in resolved);
                return;
            }

            if (resolved.Amount <= 0f)
                return;

            currentHealth = Mathf.Max(0f, currentHealth - resolved.Amount);
            invulnerability.Trigger(invulnerabilityDuration);
            Damaged?.Invoke(this, resolved);
            CombatHitFeedback.PlayDamageHit(in resolved);

            if (!IsAlive)
                Died?.Invoke(this);
        }

        public void RestoreFullHealth()
        {
            currentHealth = maxHealth;
            invulnerability.Clear();
        }

        private void Update()
        {
            invulnerability.Tick(Time.deltaTime);
        }
    }
}
