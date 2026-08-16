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

        [Test]
        public void MovementLock_DependsOnActiveStrikeDefinition()
        {
            var go = new GameObject(nameof(MovementLock_DependsOnActiveStrikeDefinition));
            var gate = go.AddComponent<PlayerActionCoordinator>();
            var melee = go.AddComponent<MeleeStrikeController>();
            var weapon = ScriptableObject.CreateInstance<MeleeWeaponConfig>();
            weapon.SetStrikes(
                new MeleeStrikeDefinition("Light", strikeDuration: 1.0f, movementLockDuration: 0.2f),
                new MeleeStrikeDefinition("Heavy", strikeDuration: 1.2f, movementLockDuration: 0.8f));
            melee.ApplyConfig(weapon, go.transform, CombatTeam.Player);

            Assert.IsTrue(melee.TryStrike(0));
            Assert.IsFalse(gate.CanMove);
            melee.TickActiveStrike(0.25f);
            Assert.IsTrue(gate.CanMove, "Light move-lock should end at 0.2s while strike continues.");
            Assert.IsTrue(melee.IsMeleeAnimActive);
            Assert.IsFalse(gate.CanAttack);
            melee.InterruptStrike();

            Assert.IsTrue(melee.TryStrike(1));
            Assert.IsFalse(gate.CanMove);
            melee.TickActiveStrike(0.25f);
            Assert.IsFalse(gate.CanMove, "Heavy move-lock should still hold at 0.25s.");
            melee.TickActiveStrike(0.6f);
            Assert.IsTrue(gate.CanMove, "Heavy move-lock should end at 0.8s while strike continues.");
            Assert.IsTrue(melee.IsMeleeAnimActive);

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void MovementLock_WhenEqualToStrikeDuration_BlocksMoveAndTurnUntilEnd()
        {
            var go = new GameObject(nameof(MovementLock_WhenEqualToStrikeDuration_BlocksMoveAndTurnUntilEnd));
            var gate = go.AddComponent<PlayerActionCoordinator>();
            var melee = go.AddComponent<MeleeStrikeController>();
            var weapon = ScriptableObject.CreateInstance<MeleeWeaponConfig>();
            weapon.SetStrikes(
                new MeleeStrikeDefinition("Light1", strikeDuration: 0.80f, movementLockDuration: 0.80f));
            melee.ApplyConfig(weapon, go.transform, CombatTeam.Player);

            Assert.IsTrue(melee.TryStrike(0));
            melee.TickActiveStrike(0.50f);
            Assert.IsTrue(melee.IsMeleeAnimActive);
            Assert.IsFalse(gate.CanMove);
            Assert.IsFalse(gate.CanTurn);

            melee.TickActiveStrike(0.40f);
            Assert.IsFalse(melee.IsMeleeAnimActive);
            Assert.IsTrue(gate.CanMove);
            Assert.IsTrue(gate.CanTurn);

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void TryStrike_InvalidIndex_ReturnsFalse()
        {
            var go = new GameObject(nameof(TryStrike_InvalidIndex_ReturnsFalse));
            go.AddComponent<PlayerActionCoordinator>();
            var melee = go.AddComponent<MeleeStrikeController>();
            var weapon = ScriptableObject.CreateInstance<MeleeWeaponConfig>();
            weapon.SetStrikes(
                new MeleeStrikeDefinition("Light", strikeDuration: 1.0f, movementLockDuration: 0.2f));
            melee.ApplyConfig(weapon, go.transform, CombatTeam.Player);

            Assert.IsFalse(melee.TryStrike(3));
            Assert.IsFalse(melee.IsMeleeAnimActive);

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(go);
        }
    }

    public sealed class MeleeStrikeStepTests
    {
        private static MeleeStrikeDefinition StepStrike(
            float distance,
            float start,
            float end,
            float duration = 0.80f) =>
            new MeleeStrikeDefinition(
                "Light1",
                strikeDuration: duration,
                movementLockDuration: 0.28f,
                strikeStepDistance: distance,
                strikeStepStart: start,
                strikeStepEnd: end);

        [Test]
        public void TryResolve_InsideWindow_UsesDistanceOverDuration()
        {
            var strike = StepStrike(distance: 0.40f, start: 0.00f, end: 0.20f);

            Assert.IsTrue(MeleeStrikeStep.TryResolvePlanarVelocity(
                strike, 0.10f, Vector3.forward, out Vector3 velocity));
            Assert.AreEqual(2f, velocity.z, 0.0001f);
            Assert.AreEqual(0f, velocity.x, 0.0001f);
        }

        [Test]
        public void TryResolve_AtWindowStart_IsActive()
        {
            var strike = StepStrike(distance: 0.40f, start: 0.02f, end: 0.22f);
            Assert.IsTrue(MeleeStrikeStep.TryResolvePlanarVelocity(
                strike, 0.02f, Vector3.forward, out _));
        }

        [Test]
        public void TryResolve_AtWindowEnd_IsInactive()
        {
            var strike = StepStrike(distance: 0.40f, start: 0.02f, end: 0.22f);
            Assert.IsFalse(MeleeStrikeStep.TryResolvePlanarVelocity(
                strike, 0.22f, Vector3.forward, out Vector3 velocity));
            Assert.AreEqual(Vector3.zero, velocity);
        }

        [Test]
        public void TryResolve_ZeroDistance_IsInactive()
        {
            var strike = StepStrike(distance: 0f, start: 0f, end: 0.20f);
            Assert.IsFalse(MeleeStrikeStep.TryResolvePlanarVelocity(
                strike, 0.10f, Vector3.forward, out _));
        }

        [Test]
        public void TryResolve_ZeroDirection_IsInactive()
        {
            var strike = StepStrike(distance: 0.40f, start: 0f, end: 0.20f);
            Assert.IsFalse(MeleeStrikeStep.TryResolvePlanarVelocity(
                strike, 0.10f, Vector3.zero, out _));
        }

        [Test]
        public void TryResolve_LaterHits_HaveDifferentSpeeds()
        {
            var light1 = StepStrike(0.42f, 0.02f, 0.22f);
            var light3 = StepStrike(0.78f, 0.04f, 0.34f, duration: 0.95f);

            Assert.IsTrue(MeleeStrikeStep.TryResolvePlanarVelocity(
                light1, 0.10f, Vector3.forward, out Vector3 first));
            Assert.IsTrue(MeleeStrikeStep.TryResolvePlanarVelocity(
                light3, 0.10f, Vector3.forward, out Vector3 third));
            Assert.Greater(third.z, first.z);
        }

        [Test]
        public void Controller_WritesPlanarVelocity_DuringStepWindow()
        {
            var go = new GameObject(nameof(Controller_WritesPlanarVelocity_DuringStepWindow));
            go.AddComponent<PlayerActionCoordinator>();
            var body = go.AddComponent<Rigidbody>();
            var melee = go.AddComponent<MeleeStrikeController>();
            var weapon = ScriptableObject.CreateInstance<MeleeWeaponConfig>();
            weapon.SetStrikes(StepStrike(distance: 0.40f, start: 0f, end: 0.20f));
            melee.ApplyConfig(weapon, go.transform, CombatTeam.Player);

            Assert.IsTrue(melee.TryStrike());
            melee.TickActiveStrike(0.10f);
            melee.TickStrikeStep();

            Assert.IsTrue(melee.IsStrikeStepActive);
            Assert.AreEqual(2f, body.linearVelocity.z, 0.0001f);

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Controller_DoesNotOverwriteVelocity_OutsideStepWindow()
        {
            var go = new GameObject(nameof(Controller_DoesNotOverwriteVelocity_OutsideStepWindow));
            go.AddComponent<PlayerActionCoordinator>();
            var body = go.AddComponent<Rigidbody>();
            var melee = go.AddComponent<MeleeStrikeController>();
            var weapon = ScriptableObject.CreateInstance<MeleeWeaponConfig>();
            weapon.SetStrikes(StepStrike(distance: 0.40f, start: 0f, end: 0.20f));
            melee.ApplyConfig(weapon, go.transform, CombatTeam.Player);

            Assert.IsTrue(melee.TryStrike());
            melee.TickActiveStrike(0.25f);
            body.linearVelocity = new Vector3(0f, 0f, 5f);
            melee.TickStrikeStep();

            Assert.IsFalse(melee.IsStrikeStepActive);
            Assert.AreEqual(5f, body.linearVelocity.z, 0.0001f);

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void TryDescribeStepTimingError_WhenEndExceedsDuration_Reports()
        {
            var strike = StepStrike(distance: 0.40f, start: 0.10f, end: 0.90f, duration: 0.80f);
            Assert.IsTrue(MeleeStrikeDefinition.TryDescribeStepTimingError(strike, out string error));
            Assert.IsTrue(error.Contains("strikeStepEnd"));
        }
    }
}
