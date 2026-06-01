using LearningArchitect.Core;
using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Tests.Core
{
    [Category("LearningArchitect.Core.Edit")]
    public sealed class ModuleRuntimeHostTests
    {
        [Test]
        public void Deactivate_IgnoresDestroyedModuleComponent()
        {
            GameObject root = new("Runtime Host Test Module");

            try
            {
                TestRuntimeModule module = root.AddComponent<TestRuntimeModule>();
                ModuleRuntimeHost host = new();
                host.Activate(new ActiveVariantHandle(root, module, module, module));

                Object.DestroyImmediate(root);

                Assert.DoesNotThrow(() => host.Deactivate());
            }
            finally
            {
                if (root != null)
                    Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ActiveItemCount_ReturnsZeroWhenStressTargetIsDestroyed()
        {
            GameObject root = new("Runtime Host Stress Test");

            try
            {
                TestRuntimeModule module = root.AddComponent<TestRuntimeModule>();
                module.ActiveCountValue = 42;

                ModuleRuntimeHost host = new();
                host.Activate(new ActiveVariantHandle(root, module, module, module));
                Object.DestroyImmediate(root);

                Assert.AreEqual(0, host.ActiveItemCount);
                Assert.DoesNotThrow(() => host.SetStressLevel(128));
            }
            finally
            {
                if (root != null)
                    Object.DestroyImmediate(root);
            }
        }

        private sealed class TestRuntimeModule : MonoBehaviour, IModule, IShowcaseStressTarget, IShowcaseMetricsSource
        {
            public int ActiveCountValue { get; set; }

            public int ActiveCount => ActiveCountValue;

            public void Enter()
            {
                gameObject.SetActive(true);
            }

            public void Exit()
            {
                gameObject.SetActive(false);
            }

            public void SetStressLevel(int count)
            {
                ActiveCountValue = count;
            }

            public ShowcaseMetricsSnapshot GetMetricsSnapshot()
            {
                return new ShowcaseMetricsSnapshot(ActiveCountValue, ActiveCountValue, 0.1f);
            }
        }
    }
}
