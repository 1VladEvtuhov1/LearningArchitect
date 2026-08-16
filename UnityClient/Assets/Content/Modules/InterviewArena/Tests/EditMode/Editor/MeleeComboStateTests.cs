using NUnit.Framework;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class MeleeComboStateTests
    {
        private static MeleeStrikeDefinition Light1() =>
            new MeleeStrikeDefinition(
                "Light1",
                strikeDuration: 0.80f,
                movementLockDuration: 0.28f,
                hitWindowStartNormalized: 0.30f,
                hitWindowEndNormalized: 0.45f,
                comboInputStart: 0.34f,
                comboInputEnd: 0.60f,
                comboTransitionTime: 0.66f,
                nextStrikeIndex: 2);

        private static MeleeStrikeDefinition Light2() =>
            new MeleeStrikeDefinition(
                "Light2",
                strikeDuration: 0.85f,
                movementLockDuration: 0.32f,
                nextStrikeIndex: MeleeStrikeDefinition.NoNextStrike);

        [Test]
        public void TryQueue_WithoutActiveWindow_Rejects()
        {
            var combo = new MeleeComboState();
            Assert.IsFalse(combo.TryQueue(Light1(), 0.10f, strikeCount: 3, out _));
            Assert.IsFalse(combo.HasQueuedStrike);
        }

        [Test]
        public void TryQueue_AtComboInputStart_Accepts()
        {
            var combo = new MeleeComboState();
            Assert.IsTrue(combo.TryQueue(Light1(), 0.34f, strikeCount: 3, out int queued));
            Assert.AreEqual(2, queued);
            Assert.IsTrue(combo.HasQueuedStrike);
        }

        [Test]
        public void TryQueue_JustBeforeComboInputEnd_Accepts()
        {
            var combo = new MeleeComboState();
            Assert.IsTrue(combo.TryQueue(Light1(), 0.599f, strikeCount: 3, out _));
        }

        [Test]
        public void TryQueue_AtComboInputEnd_RejectsHalfOpen()
        {
            var combo = new MeleeComboState();
            Assert.IsFalse(combo.TryQueue(Light1(), 0.60f, strikeCount: 3, out _));
        }

        [Test]
        public void TryQueue_SecondPress_DoesNotReplaceQueue()
        {
            var combo = new MeleeComboState();
            Assert.IsTrue(combo.TryQueue(Light1(), 0.40f, strikeCount: 3, out _));
            Assert.IsFalse(combo.TryQueue(Light1(), 0.45f, strikeCount: 3, out _));
            Assert.AreEqual(2, combo.QueuedStrikeIndex);
        }

        [Test]
        public void ShouldTransition_OnlyAfterComboTransitionTime()
        {
            var combo = new MeleeComboState();
            Assert.IsTrue(combo.TryQueue(Light1(), 0.40f, strikeCount: 3, out _));
            Assert.IsFalse(combo.ShouldTransition(Light1(), 0.65f));
            Assert.IsTrue(combo.ShouldTransition(Light1(), 0.66f));
        }

        [Test]
        public void TryQueue_FinalStrike_Rejects()
        {
            var combo = new MeleeComboState();
            Assert.IsFalse(combo.TryQueue(Light2(), 0.40f, strikeCount: 3, out _));
        }

        [Test]
        public void TryQueue_InvalidNextIndex_Rejects()
        {
            var combo = new MeleeComboState();
            var broken = new MeleeStrikeDefinition(
                "Broken",
                strikeDuration: 0.80f,
                movementLockDuration: 0.2f,
                comboInputStart: 0.3f,
                comboInputEnd: 0.5f,
                comboTransitionTime: 0.6f,
                nextStrikeIndex: 9);
            Assert.IsFalse(combo.TryQueue(broken, 0.40f, strikeCount: 3, out _));
        }

        [Test]
        public void Clear_ResetsQueuedState()
        {
            var combo = new MeleeComboState();
            Assert.IsTrue(combo.TryQueue(Light1(), 0.40f, strikeCount: 3, out _));
            combo.Clear();
            Assert.IsFalse(combo.HasQueuedStrike);
            Assert.AreEqual(-1, combo.QueuedStrikeIndex);
        }

        [Test]
        public void TryConsumeQueue_ClearsAndReturnsIndex()
        {
            var combo = new MeleeComboState();
            Assert.IsTrue(combo.TryQueue(Light1(), 0.40f, strikeCount: 3, out _));
            Assert.IsTrue(combo.TryConsumeQueue(out int index));
            Assert.AreEqual(2, index);
            Assert.IsFalse(combo.HasQueuedStrike);
        }

        [Test]
        public void HasValidComboTimings_WhenNoNext_AllowsZeroWindow()
        {
            Assert.IsTrue(Light2().HasValidComboTimings);
        }

        [Test]
        public void HasValidComboTimings_WhenNext_RequiresOrderedSeconds()
        {
            var bad = new MeleeStrikeDefinition(
                "Bad",
                strikeDuration: 0.80f,
                movementLockDuration: 0.2f,
                comboInputStart: 0.50f,
                comboInputEnd: 0.40f,
                comboTransitionTime: 0.60f,
                nextStrikeIndex: 2);
            Assert.IsFalse(bad.HasValidComboTimings);
            Assert.IsTrue(Light1().HasValidComboTimings);
        }
    }
}
