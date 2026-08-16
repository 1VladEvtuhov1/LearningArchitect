using NUnit.Framework;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class PlayerVitalHudRulesTests
    {
        [Test]
        public void Normalized_WhenFull_ReturnsOne()
        {
            Assert.AreEqual(1f, PlayerVitalHudRules.Normalized(100f, 100f));
        }

        [Test]
        public void Normalized_WhenHalf_ReturnsHalf()
        {
            Assert.AreEqual(0.5f, PlayerVitalHudRules.Normalized(50f, 100f));
        }

        [Test]
        public void Normalized_WhenMaxIsZero_ReturnsZero()
        {
            Assert.AreEqual(0f, PlayerVitalHudRules.Normalized(10f, 0f));
        }

        [Test]
        public void FormatLabel_UsesCeilAndClampsCurrent()
        {
            Assert.AreEqual("80/100", PlayerVitalHudRules.FormatLabel(80f, 100f));
            Assert.AreEqual("0/100", PlayerVitalHudRules.FormatLabel(-4f, 100f));
        }
    }
}
