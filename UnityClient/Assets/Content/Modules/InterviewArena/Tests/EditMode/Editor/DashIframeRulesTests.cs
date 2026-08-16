using LearningArchitect.Modules.InterviewArena;
using NUnit.Framework;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class DashIframeRulesTests
    {
        [Test]
        public void IsActive_StartAndTail_AreVulnerable()
        {
            Assert.IsFalse(DashIframeRules.IsActive(0f, 1f, 0.85f));
            Assert.IsFalse(DashIframeRules.IsActive(0.074f, 1f, 0.85f));
            Assert.IsFalse(DashIframeRules.IsActive(0.926f, 1f, 0.85f));
            Assert.IsFalse(DashIframeRules.IsActive(1f, 1f, 0.85f));
        }

        [Test]
        public void IsActive_MiddleCoverage_Absorbs()
        {
            Assert.IsTrue(DashIframeRules.IsActive(0.075f, 1f, 0.85f));
            Assert.IsTrue(DashIframeRules.IsActive(0.5f, 1f, 0.85f));
            Assert.IsTrue(DashIframeRules.IsActive(0.925f, 1f, 0.85f));
        }

        [Test]
        public void IsActive_ZeroDurationOrCoverage_IsFalse()
        {
            Assert.IsFalse(DashIframeRules.IsActive(0f, 0f, 0.85f));
            Assert.IsFalse(DashIframeRules.IsActive(0.5f, 1f, 0f));
        }

        [Test]
        public void IsActive_FullCoverage_IncludesDashStart()
        {
            Assert.IsTrue(DashIframeRules.IsActive(0f, 1f, 1f));
            Assert.IsTrue(DashIframeRules.IsActive(1f, 1f, 1f));
        }
    }
}
