using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class EnemyMeleeAttackTests
    {
        [Test]
        public void TryBeginAttack_IncrementsMeleeAnimStartVersion()
        {
            var go = new GameObject(nameof(TryBeginAttack_IncrementsMeleeAnimStartVersion));
            EnemyConfig config = ScriptableObject.CreateInstance<EnemyConfig>();
            try
            {
                EnemyMeleeAttack melee = go.AddComponent<EnemyMeleeAttack>();
                Assert.AreEqual(0, melee.MeleeAnimStartVersion);
                Assert.AreEqual("ComboRight1", melee.ActiveMeleeAnimTrigger);

                Assert.IsTrue(melee.TryBeginAttack(config));
                Assert.AreEqual(1, melee.MeleeAnimStartVersion);
                Assert.IsTrue(melee.IsBusy);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void Interrupt_DuringWindup_PreventsStrikeAndClearsBusy()
        {
            var go = new GameObject(nameof(Interrupt_DuringWindup_PreventsStrikeAndClearsBusy));
            EnemyConfig config = ScriptableObject.CreateInstance<EnemyConfig>();
            try
            {
                EnemyMeleeAttack melee = go.AddComponent<EnemyMeleeAttack>();
                Assert.IsTrue(melee.TryBeginAttack(config));

                melee.Interrupt();
                melee.Tick(config, 0, 999f);

                Assert.IsFalse(melee.IsBusy);
                Assert.AreEqual(1, melee.MeleeAnimStartVersion);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void TryBeginAttack_WhenBusy_DoesNotRetrigger()
        {
            var go = new GameObject(nameof(TryBeginAttack_WhenBusy_DoesNotRetrigger));
            EnemyConfig config = ScriptableObject.CreateInstance<EnemyConfig>();
            try
            {
                EnemyMeleeAttack melee = go.AddComponent<EnemyMeleeAttack>();
                Assert.IsTrue(melee.TryBeginAttack(config));
                Assert.IsFalse(melee.TryBeginAttack(config));
                Assert.AreEqual(1, melee.MeleeAnimStartVersion);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(config);
            }
        }
    }
}
