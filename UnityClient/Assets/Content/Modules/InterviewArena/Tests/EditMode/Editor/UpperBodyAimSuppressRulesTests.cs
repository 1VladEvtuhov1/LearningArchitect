using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class UpperBodyAimSuppressRulesTests
    {
        [Test]
        public void HasLock_DetectsHitstunOnly()
        {
            var go = new GameObject(nameof(HasLock_DetectsHitstunOnly));
            var gate = go.AddComponent<PlayerActionCoordinator>();

            Assert.IsFalse(gate.HasLock(CharacterActionKind.Hitstun));
            gate.ApplyHitstun(0.3f);
            Assert.IsTrue(gate.HasLock(CharacterActionKind.Hitstun));
            Assert.IsFalse(gate.HasLock(CharacterActionKind.Dash));

            Object.DestroyImmediate(go);
        }
    }
}
