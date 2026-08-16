using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class MeleeComboTransitionTests
    {
        private static MeleeWeaponConfig CreateComboWeapon()
        {
            var weapon = ScriptableObject.CreateInstance<MeleeWeaponConfig>();
            weapon.SetStrikes(
                new MeleeStrikeDefinition(
                    "Light1",
                    strikeDuration: 0.80f,
                    movementLockDuration: 0.28f,
                    hitWindowStartNormalized: 0.30f,
                    hitWindowEndNormalized: 0.45f,
                    animTriggerOverride: "ComboRight1",
                    comboInputStart: 0.34f,
                    comboInputEnd: 0.60f,
                    comboTransitionTime: 0.66f,
                    nextStrikeIndex: 2),
                new MeleeStrikeDefinition(
                    "Heavy",
                    strikeDuration: 1.35f,
                    movementLockDuration: 0.85f,
                    nextStrikeIndex: MeleeStrikeDefinition.NoNextStrike),
                new MeleeStrikeDefinition(
                    "Light2",
                    strikeDuration: 0.85f,
                    movementLockDuration: 0.32f,
                    animTriggerOverride: "ComboRight2",
                    comboInputStart: 0.36f,
                    comboInputEnd: 0.64f,
                    comboTransitionTime: 0.70f,
                    nextStrikeIndex: 3),
                new MeleeStrikeDefinition(
                    "Light3",
                    strikeDuration: 0.95f,
                    movementLockDuration: 0.40f,
                    animTriggerOverride: "ComboRight3",
                    nextStrikeIndex: MeleeStrikeDefinition.NoNextStrike));
            return weapon;
        }

        [Test]
        public void EarlyComboInput_IsRejected_AndDoesNotStartNextStrike()
        {
            var go = new GameObject(nameof(EarlyComboInput_IsRejected_AndDoesNotStartNextStrike));
            go.AddComponent<PlayerActionCoordinator>();
            var melee = go.AddComponent<MeleeStrikeController>();
            var weapon = CreateComboWeapon();
            melee.ApplyConfig(weapon, go.transform, CombatTeam.Player);

            Assert.AreEqual(AttackInputResult.StartedStrike, melee.HandleMeleeAttackInput());
            Assert.AreEqual(AttackInputResult.Rejected, melee.HandleMeleeAttackInput());
            Assert.AreEqual(0, melee.ActiveStrikeIndex);

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void QueuedStrike_StartsAtTransitionTime_NotOnPress()
        {
            var go = new GameObject(nameof(QueuedStrike_StartsAtTransitionTime_NotOnPress));
            go.AddComponent<PlayerActionCoordinator>();
            var melee = go.AddComponent<MeleeStrikeController>();
            var weapon = CreateComboWeapon();
            melee.ApplyConfig(weapon, go.transform, CombatTeam.Player);

            Assert.AreEqual(AttackInputResult.StartedStrike, melee.HandleMeleeAttackInput());
            melee.TickActiveStrike(0.40f);
            Assert.AreEqual(AttackInputResult.QueuedNextStrike, melee.HandleMeleeAttackInput());
            Assert.AreEqual(0, melee.ActiveStrikeIndex);
            Assert.AreEqual(1, melee.MeleeAnimStartVersion);

            Assert.AreEqual(AttackInputResult.Rejected, melee.HandleMeleeAttackInput());

            melee.TickActiveStrike(0.25f); // 0.65
            Assert.AreEqual(0, melee.ActiveStrikeIndex);

            melee.TickActiveStrike(0.02f); // 0.67
            Assert.AreEqual(2, melee.ActiveStrikeIndex);
            Assert.AreEqual(2, melee.MeleeAnimStartVersion);
            Assert.AreEqual("ComboRight2", melee.ActiveMeleeAnimTrigger);
            Assert.IsTrue(melee.IsStrikeActive);

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void AfterTransition_NewPressRequired_QueueDoesNotCarry()
        {
            var go = new GameObject(nameof(AfterTransition_NewPressRequired_QueueDoesNotCarry));
            go.AddComponent<PlayerActionCoordinator>();
            var melee = go.AddComponent<MeleeStrikeController>();
            var weapon = CreateComboWeapon();
            melee.ApplyConfig(weapon, go.transform, CombatTeam.Player);

            Assert.AreEqual(AttackInputResult.StartedStrike, melee.HandleMeleeAttackInput());
            melee.TickActiveStrike(0.40f);
            Assert.AreEqual(AttackInputResult.QueuedNextStrike, melee.HandleMeleeAttackInput());
            melee.TickActiveStrike(0.30f);
            Assert.AreEqual(2, melee.ActiveStrikeIndex);

            Assert.AreEqual(AttackInputResult.Rejected, melee.HandleMeleeAttackInput());

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Light2_QueuesLight3_WithComboRight3Trigger()
        {
            var go = new GameObject(nameof(Light2_QueuesLight3_WithComboRight3Trigger));
            go.AddComponent<PlayerActionCoordinator>();
            var melee = go.AddComponent<MeleeStrikeController>();
            var weapon = CreateComboWeapon();
            melee.ApplyConfig(weapon, go.transform, CombatTeam.Player);

            Assert.AreEqual(AttackInputResult.StartedStrike, melee.HandleMeleeAttackInput());
            melee.TickActiveStrike(0.40f);
            Assert.AreEqual(AttackInputResult.QueuedNextStrike, melee.HandleMeleeAttackInput());
            melee.TickActiveStrike(0.30f);
            Assert.AreEqual(2, melee.ActiveStrikeIndex);
            Assert.AreEqual("ComboRight2", melee.ActiveMeleeAnimTrigger);

            melee.TickActiveStrike(0.40f);
            Assert.AreEqual(AttackInputResult.QueuedNextStrike, melee.HandleMeleeAttackInput());
            melee.TickActiveStrike(0.31f);
            Assert.AreEqual(3, melee.ActiveStrikeIndex);
            Assert.AreEqual("ComboRight3", melee.ActiveMeleeAnimTrigger);
            Assert.AreEqual(AttackInputResult.Rejected, melee.HandleMeleeAttackInput());

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void MissedComboWindow_ContinuesUntilStrikeDuration()
        {
            var go = new GameObject(nameof(MissedComboWindow_ContinuesUntilStrikeDuration));
            go.AddComponent<PlayerActionCoordinator>();
            var melee = go.AddComponent<MeleeStrikeController>();
            var weapon = CreateComboWeapon();
            melee.ApplyConfig(weapon, go.transform, CombatTeam.Player);

            Assert.AreEqual(AttackInputResult.StartedStrike, melee.HandleMeleeAttackInput());
            melee.TickActiveStrike(0.70f);
            Assert.IsTrue(melee.IsStrikeActive);
            Assert.AreEqual(0, melee.ActiveStrikeIndex);

            melee.TickActiveStrike(0.15f);
            Assert.IsFalse(melee.IsStrikeActive);

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void InterruptStrike_ClearsQueuedCombo()
        {
            var go = new GameObject(nameof(InterruptStrike_ClearsQueuedCombo));
            go.AddComponent<PlayerActionCoordinator>();
            var melee = go.AddComponent<MeleeStrikeController>();
            var weapon = CreateComboWeapon();
            melee.ApplyConfig(weapon, go.transform, CombatTeam.Player);

            Assert.AreEqual(AttackInputResult.StartedStrike, melee.HandleMeleeAttackInput());
            melee.TickActiveStrike(0.40f);
            Assert.AreEqual(AttackInputResult.QueuedNextStrike, melee.HandleMeleeAttackInput());

            melee.InterruptStrike();
            Assert.IsFalse(melee.IsStrikeActive);

            Assert.AreEqual(AttackInputResult.StartedStrike, melee.HandleMeleeAttackInput());
            Assert.AreEqual(0, melee.ActiveStrikeIndex);

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Transition_ReplacesMeleeLock_WithoutAttackGap()
        {
            var go = new GameObject(nameof(Transition_ReplacesMeleeLock_WithoutAttackGap));
            var gate = go.AddComponent<PlayerActionCoordinator>();
            var melee = go.AddComponent<MeleeStrikeController>();
            var weapon = CreateComboWeapon();
            melee.ApplyConfig(weapon, go.transform, CombatTeam.Player);

            Assert.AreEqual(AttackInputResult.StartedStrike, melee.HandleMeleeAttackInput());
            melee.TickActiveStrike(0.40f);
            Assert.AreEqual(AttackInputResult.QueuedNextStrike, melee.HandleMeleeAttackInput());
            melee.TickActiveStrike(0.30f);

            Assert.AreEqual(2, melee.ActiveStrikeIndex);
            Assert.IsFalse(gate.CanAttack);

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void BowAimStance_InterruptsActiveStrike()
        {
            var go = new GameObject(nameof(BowAimStance_InterruptsActiveStrike));
            go.AddComponent<PlayerActionCoordinator>();
            go.AddComponent<PlayerInputReader>();
            var melee = go.AddComponent<MeleeStrikeController>();
            var stance = go.AddComponent<PlayerCombatStance>();
            var weapon = CreateComboWeapon();
            melee.ApplyConfig(weapon, go.transform, CombatTeam.Player);

            Assert.AreEqual(AttackInputResult.StartedStrike, melee.HandleMeleeAttackInput());
            stance.SetStance(ArenaCombatStance.MeleeReady);
            Assert.IsTrue(melee.IsStrikeActive);

            stance.SetStance(ArenaCombatStance.BowAim);
            Assert.IsFalse(melee.IsStrikeActive);

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(go);
        }
    }
}
