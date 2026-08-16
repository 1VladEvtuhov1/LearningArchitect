using NUnit.Framework;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class PlayerCombatMeleeInputRulesTests
    {
        [Test]
        public void ShouldConsume_WhenStrikeActive_EvenIfCanAttackFalse()
        {
            Assert.IsTrue(PlayerCombatMeleeInputRules.ShouldConsumeMeleePress(
                isStrikeActive: true,
                canAttack: false));
        }

        [Test]
        public void ShouldNotConsume_WhenIdleAndCanAttackFalse_PreservesForgiveness()
        {
            Assert.IsFalse(PlayerCombatMeleeInputRules.ShouldConsumeMeleePress(
                isStrikeActive: false,
                canAttack: false));
        }

        [Test]
        public void ShouldConsume_WhenIdleAndCanAttackTrue()
        {
            Assert.IsTrue(PlayerCombatMeleeInputRules.ShouldConsumeMeleePress(
                isStrikeActive: false,
                canAttack: true));
        }

        [Test]
        public void ShouldConsume_WhenBlockingEvenIfCanAttackFalse()
        {
            Assert.IsTrue(PlayerCombatMeleeInputRules.ShouldConsumeMeleePress(
                isStrikeActive: false,
                canAttack: false,
                isBlocking: true));
        }
    }
}
