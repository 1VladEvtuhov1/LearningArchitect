using NUnit.Framework;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class CharacterActionGateRulesTests
    {
        [Test]
        public void CanMove_WhenNoLocks_ReturnsTrue()
        {
            var locks = new CharacterActionLock[4];
            Assert.IsTrue(CharacterActionGateRules.CanMove(locks, 0));
        }

        [Test]
        public void CanMove_WhenDashLockActive_ReturnsFalse()
        {
            var locks = new[] { CharacterActionLock.Dash(0.2f) };
            Assert.IsFalse(CharacterActionGateRules.CanMove(locks, locks.Length));
            Assert.IsFalse(CharacterActionGateRules.CanAttack(locks, locks.Length));
        }

        [Test]
        public void CanMove_WhenMeleeStartupOnlyBlocksMovement_AllowsAttackAfterStartup()
        {
            var locks = new[] { CharacterActionLock.MeleeStrike(1f, inMovementStartup: false) };
            Assert.IsTrue(CharacterActionGateRules.CanMove(locks, locks.Length));
            Assert.IsFalse(CharacterActionGateRules.CanAttack(locks, locks.Length));
        }

        [Test]
        public void Hitstun_BlocksAllCapabilities()
        {
            var locks = new[] { CharacterActionLock.Hitstun(0.3f) };
            Assert.IsFalse(CharacterActionGateRules.CanMove(locks, locks.Length));
            Assert.IsFalse(CharacterActionGateRules.CanTurn(locks, locks.Length));
            Assert.IsFalse(CharacterActionGateRules.CanAttack(locks, locks.Length));
            Assert.IsFalse(CharacterActionGateRules.CanDash(locks, locks.Length));
            Assert.IsFalse(CharacterActionGateRules.CanJump(locks, locks.Length));
            Assert.IsTrue(CharacterActionGateRules.HasActiveAction(locks, locks.Length));
        }
    }
}
