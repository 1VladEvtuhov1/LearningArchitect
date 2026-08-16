using NUnit.Framework;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class PlayerCombatCrossbowInputRulesTests
    {
        [Test]
        public void ShouldConsume_WhenShotActive_EvenIfCannotAttack()
        {
            Assert.IsTrue(PlayerCombatCrossbowInputRules.ShouldConsumeCrossbowPress(true, false));
        }

        [Test]
        public void ShouldConsume_WhenIdleAndCanAttack()
        {
            Assert.IsTrue(PlayerCombatCrossbowInputRules.ShouldConsumeCrossbowPress(false, true));
        }

        [Test]
        public void ShouldNotConsume_WhenIdleAndCannotAttack()
        {
            Assert.IsFalse(PlayerCombatCrossbowInputRules.ShouldConsumeCrossbowPress(false, false));
        }
    }
}
