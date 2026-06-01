using LearningArchitect.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LearningArchitect.Tests.Core
{
    [Category("LearningArchitect.Core.Edit")]
    public sealed class ShowcaseLayoutToolTests
    {
        private const string ExpectedMessage = "Showcase layout rebuild is deprecated and disabled. Use Assets/Content/Showcase/Prefabs/ArchitectureShowcaseHub.prefab as the source of truth, or inspect Assets/Content/Showcase/Prefabs/Archive/ArchitectureShowcaseHub_LegacyArchive.prefab as an archived baseline until a new explicit tool is designed.";

        [Test]
        public void Rebuild_ReturnsSafeModeMessage_AndLogsWarning()
        {
            LogAssert.Expect(LogType.Warning, ExpectedMessage);

            string result = ShowcaseLayoutTool.Rebuild();

            Assert.That(result, Is.EqualTo(ExpectedMessage));
        }

        [Test]
        public void ApplyToScene_ReturnsSafeModeMessage_AndLogsWarning()
        {
            LogAssert.Expect(LogType.Warning, ExpectedMessage);

            string result = ShowcaseLayoutTool.ApplyToScene();

            Assert.That(result, Is.EqualTo(ExpectedMessage));
        }
    }
}
