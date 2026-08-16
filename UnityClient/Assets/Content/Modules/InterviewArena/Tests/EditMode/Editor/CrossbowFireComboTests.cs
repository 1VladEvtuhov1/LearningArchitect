using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class CrossbowFireComboTests
    {
        [Test]
        public void Combo_QueuesInsideWindow_AndTransitions()
        {
            var shot0 = new CrossbowShotDefinition("S0", 0.55f, 0.35f, 0.18f, 0.40f, 0.40f, 1);
            var combo = new MeleeComboState();

            Assert.IsTrue(combo.TryQueue(shot0, 0.25f, 2, out int queued));
            Assert.AreEqual(1, queued);
            Assert.IsTrue(combo.ShouldTransition(shot0, 0.40f));
            Assert.IsTrue(combo.TryConsumeQueue(out int next));
            Assert.AreEqual(1, next);
        }

        [Test]
        public void Combo_RejectsOutsideWindow()
        {
            var shot0 = new CrossbowShotDefinition("S0", 0.55f, 0.35f, 0.18f, 0.40f, 0.40f, 1);
            var combo = new MeleeComboState();

            Assert.IsFalse(combo.TryQueue(shot0, 0.10f, 2, out _));
            Assert.IsFalse(combo.TryQueue(shot0, 0.45f, 2, out _));
        }

        [Test]
        public void FinalShot_HasNoComboChain()
        {
            var shot1 = new CrossbowShotDefinition("S1", 0.55f, 0.35f);
            var combo = new MeleeComboState();

            Assert.IsTrue(shot1.HasValidComboTimings);
            Assert.IsFalse(combo.TryQueue(shot1, 0.25f, 2, out _));
        }

        [Test]
        public void CrossbowFireLock_BlocksMovementDuringMoveLock()
        {
            var go = new GameObject(nameof(CrossbowFireLock_BlocksMovementDuringMoveLock));
            var gate = go.AddComponent<PlayerActionCoordinator>();

            gate.SetLock(CharacterActionLock.CrossbowFire(0.35f, inMovementLock: true));
            Assert.IsTrue(gate.HasLock(CharacterActionKind.CrossbowFire));
            Assert.IsFalse(gate.CanMove);
            Assert.IsFalse(gate.CanAttack);

            gate.SetLock(CharacterActionLock.CrossbowFire(0.2f, inMovementLock: false));
            Assert.IsTrue(gate.CanMove);
            Assert.IsFalse(gate.CanAttack);

            Object.DestroyImmediate(go);
        }
    }
}
