using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class PlayerDeathRespawnRulesTests
    {
        [Test]
        public void ShouldLocalRespawn_WhenNoOnlineMatch_ReturnsTrue()
        {
            Assert.IsTrue(PlayerDeathRespawnRules.ShouldLocalRespawn(false));
        }

        [Test]
        public void ShouldLocalRespawn_WhenOnlineMatchActive_ReturnsFalse()
        {
            Assert.IsFalse(PlayerDeathRespawnRules.ShouldLocalRespawn(true));
        }

        [Test]
        public void ResolveDelay_WhenUnset_UsesDefault()
        {
            Assert.AreEqual(
                PlayerDeathRespawnRules.DefaultDelaySeconds,
                PlayerDeathRespawnRules.ResolveDelay(0f));
        }

        [Test]
        public void TickDue_AfterDelayElapses_ReturnsTrueOnce()
        {
            float timer = 0.4f;
            Assert.IsFalse(PlayerDeathRespawnRules.TickDue(ref timer, 0.2f));
            Assert.IsTrue(PlayerDeathRespawnRules.TickDue(ref timer, 0.3f));
            Assert.IsFalse(PlayerDeathRespawnRules.TickDue(ref timer, 0.3f));
            Assert.AreEqual(-1f, timer);
        }

        [Test]
        public void RestoreFullHealth_AfterLethalHit_ReturnsToMax()
        {
            var go = new GameObject(nameof(RestoreFullHealth_AfterLethalHit_ReturnsToMax));
            try
            {
                Health health = go.AddComponent<Health>();
                health.ApplyConfig(CombatTeam.Player, 10f, 0f);
                health.ApplyDamage(new DamageInfo(10f, CombatTeam.Enemy, Vector3.zero, Vector3.forward));

                Assert.IsFalse(health.IsAlive);
                Assert.AreEqual(0f, health.CurrentHealth);

                health.RestoreFullHealth();

                Assert.IsTrue(health.IsAlive);
                Assert.AreEqual(10f, health.CurrentHealth);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
