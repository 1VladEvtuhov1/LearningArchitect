using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class MeleeStrikeInterruptTests
    {
        [Test]
        public void TryStrike_AfterInterrupt_IncrementsMeleeAnimStartVersion()
        {
            var go = new GameObject(nameof(TryStrike_AfterInterrupt_IncrementsMeleeAnimStartVersion));
            go.AddComponent<PlayerActionCoordinator>();
            var melee = go.AddComponent<MeleeStrikeController>();
            var weapon = ScriptableObject.CreateInstance<MeleeWeaponConfig>();
            melee.ApplyConfig(weapon, go.transform, CombatTeam.Player);

            Assert.AreEqual(0, melee.MeleeAnimStartVersion);
            Assert.IsTrue(melee.TryStrike());
            Assert.AreEqual(1, melee.MeleeAnimStartVersion);

            melee.InterruptStrike();
            Assert.IsTrue(melee.TryStrike());
            Assert.AreEqual(2, melee.MeleeAnimStartVersion);
            Assert.IsTrue(melee.IsMeleeAnimActive);

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void InterruptStrike_WhenActive_ClearsAnimFlagAndMeleeLock()
        {
            var go = new GameObject(nameof(InterruptStrike_WhenActive_ClearsAnimFlagAndMeleeLock));
            go.AddComponent<PlayerActionCoordinator>();
            var melee = go.AddComponent<MeleeStrikeController>();
            var weapon = ScriptableObject.CreateInstance<MeleeWeaponConfig>();
            melee.ApplyConfig(weapon, go.transform, CombatTeam.Player);

            Assert.IsTrue(melee.TryStrike());
            Assert.IsTrue(melee.IsMeleeAnimActive);

            var gate = go.GetComponent<PlayerActionCoordinator>();
            Assert.IsFalse(gate.CanAttack);

            melee.InterruptStrike();

            Assert.IsFalse(melee.IsMeleeAnimActive);
            Assert.IsTrue(gate.CanAttack);

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void InterruptStrike_WhenIdle_IsNoOp()
        {
            var go = new GameObject(nameof(InterruptStrike_WhenIdle_IsNoOp));
            go.AddComponent<PlayerActionCoordinator>();
            var melee = go.AddComponent<MeleeStrikeController>();

            Assert.IsFalse(melee.IsMeleeAnimActive);
            melee.InterruptStrike();
            Assert.IsFalse(melee.IsMeleeAnimActive);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ApplyHitstun_AfterInterrupt_BlocksAllCapabilities()
        {
            var go = new GameObject(nameof(ApplyHitstun_AfterInterrupt_BlocksAllCapabilities));
            var gate = go.AddComponent<PlayerActionCoordinator>();
            var melee = go.AddComponent<MeleeStrikeController>();
            var weapon = ScriptableObject.CreateInstance<MeleeWeaponConfig>();
            melee.ApplyConfig(weapon, go.transform, CombatTeam.Player);

            Assert.IsTrue(melee.TryStrike());
            melee.InterruptStrike();
            gate.ApplyHitstun(0.35f);

            Assert.IsFalse(melee.IsMeleeAnimActive);
            Assert.IsFalse(gate.CanMove);
            Assert.IsFalse(gate.CanTurn);
            Assert.IsFalse(gate.CanAttack);
            Assert.IsFalse(gate.CanDash);
            Assert.IsFalse(gate.CanJump);

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(go);
        }
    }
}
