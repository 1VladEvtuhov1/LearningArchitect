using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class EnemyRespawnVisualRulesTests
    {
        [Test]
        public void CollectManagedRenderers_ExcludesPhysicsCapsuleProxy()
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = nameof(CollectManagedRenderers_ExcludesPhysicsCapsuleProxy);

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "HumanoidVisual";
            visual.transform.SetParent(root.transform, false);

            MeshRenderer capsuleRenderer = root.GetComponent<MeshRenderer>();
            MeshRenderer visualRenderer = visual.GetComponent<MeshRenderer>();
            Assert.IsNotNull(capsuleRenderer);
            Assert.IsNotNull(visualRenderer);

            capsuleRenderer.enabled = false;
            visualRenderer.enabled = true;

            Renderer[] managed = EnemyRespawnVisualRules.CollectManagedRenderers(root.transform);

            Assert.AreEqual(1, managed.Length);
            Assert.AreSame(visualRenderer, managed[0]);
            Assert.IsTrue(EnemyRespawnVisualRules.IsPhysicsProxyRenderer(capsuleRenderer, root.transform));
            Assert.IsFalse(EnemyRespawnVisualRules.IsPhysicsProxyRenderer(visualRenderer, root.transform));

            Object.DestroyImmediate(root);
        }
    }
}
