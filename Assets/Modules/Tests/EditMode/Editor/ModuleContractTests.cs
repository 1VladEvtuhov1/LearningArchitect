using System;
using System.Reflection;
using LearningArchitect.Core;
using LearningArchitect.Modules.Animation3D;
using LearningArchitect.Modules.Effects;
using LearningArchitect.Modules.Inventory;
using LearningArchitect.Modules.Performance;
using LearningArchitect.Modules.Pooling;
using LearningArchitect.Modules.VFX;
using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Tests.Modules
{
    [Category("LearningArchitect.Modules.Edit")]
    public sealed class ModuleContractTests
    {
        [Test]
        public void InstantiationVariant_SeparatesSimulationAndVisibleCounts()
        {
            using ModuleFixture<InstantiationVariant> fixture = CreateFixture<InstantiationVariant>();

            fixture.InvokeLifecycle("Awake");
            fixture.Component.SetStressLevel(4000);
            fixture.InvokeLifecycle("Update");

            ShowcaseMetricsSnapshot metrics = fixture.Component.GetMetricsSnapshot();

            Assert.AreEqual(4000, metrics.SimulationCount);
            Assert.AreEqual(200, metrics.VisibleCount);
            Assert.AreEqual(fixture.Component.ActiveCount, metrics.VisibleCount);
            Assert.AreEqual(200, fixture.Root.transform.childCount);
        }

        [Test]
        public void PooledVariant_UsesStressLevelForSimulationCountAndPoolBudgetForVisibleCount()
        {
            using ModuleFixture<PooledVariant> fixture = CreateFixture<PooledVariant>();

            fixture.InvokeLifecycle("Awake");
            fixture.Component.SetStressLevel(4000);
            fixture.InvokeLifecycle("Update");

            ShowcaseMetricsSnapshot metrics = fixture.Component.GetMetricsSnapshot();

            Assert.AreEqual(4000, metrics.SimulationCount);
            Assert.AreEqual(200, metrics.VisibleCount);
            Assert.AreEqual(fixture.Component.ActiveCount, metrics.VisibleCount);
            Assert.AreEqual(240, fixture.Root.transform.childCount);
        }

        [Test]
        public void InventoryVariants_ReportLogicalSlotCountAndMappedVisibleCounts()
        {
            using ModuleFixture<InventoryPackedVariant> packed = CreateFixture<InventoryPackedVariant>();
            using ModuleFixture<InventoryObjectsVariant> objects = CreateFixture<InventoryObjectsVariant>();

            packed.InvokeLifecycle("Awake");
            objects.InvokeLifecycle("Awake");

            ShowcaseMetricsSnapshot packedMetrics = packed.Component.GetMetricsSnapshot();
            ShowcaseMetricsSnapshot objectMetrics = objects.Component.GetMetricsSnapshot();

            Assert.AreEqual(4096, packedMetrics.SimulationCount);
            Assert.AreEqual(240, packedMetrics.VisibleCount);
            Assert.AreEqual(packed.Component.ActiveCount, packedMetrics.VisibleCount);

            Assert.AreEqual(1024, objectMetrics.SimulationCount);
            Assert.AreEqual(220, objectMetrics.VisibleCount);
            Assert.AreEqual(objects.Component.ActiveCount, objectMetrics.VisibleCount);
        }

        [Test]
        public void PerformanceVariants_CapVisibleActorsButKeepRequestedSimulationCount()
        {
            using ModuleFixture<CentralizedUpdateVariant> centralized = CreateFixture<CentralizedUpdateVariant>();
            using ModuleFixture<PerObjectUpdateVariant> perObject = CreateFixture<PerObjectUpdateVariant>();

            centralized.InvokeLifecycle("Awake");
            perObject.InvokeLifecycle("Awake");

            ShowcaseMetricsSnapshot centralizedMetrics = centralized.Component.GetMetricsSnapshot();
            ShowcaseMetricsSnapshot perObjectMetrics = perObject.Component.GetMetricsSnapshot();

            Assert.AreEqual(5000, centralizedMetrics.SimulationCount);
            Assert.AreEqual(260, centralizedMetrics.VisibleCount);
            Assert.AreEqual(centralized.Component.ActiveCount, centralizedMetrics.VisibleCount);

            Assert.AreEqual(5000, perObjectMetrics.SimulationCount);
            Assert.AreEqual(260, perObjectMetrics.VisibleCount);
            Assert.AreEqual(perObject.Component.ActiveCount, perObjectMetrics.VisibleCount);
            Assert.AreEqual(260, perObject.Root.transform.childCount);
        }

        [Test]
        public void ChunkEffectsVariant_ScalesVisualBudgetWithinConfiguredBounds()
        {
            using ModuleFixture<ChunkEffectsVariant> fixture = CreateFixture<ChunkEffectsVariant>();

            fixture.InvokeLifecycle("Awake");

            ShowcaseMetricsSnapshot defaultMetrics = fixture.Component.GetMetricsSnapshot();
            Assert.AreEqual(5000, defaultMetrics.SimulationCount);
            Assert.AreEqual(420, defaultMetrics.VisibleCount);

            fixture.Component.SetStressLevel(100);
            ShowcaseMetricsSnapshot reducedMetrics = fixture.Component.GetMetricsSnapshot();

            Assert.AreEqual(100, reducedMetrics.SimulationCount);
            Assert.AreEqual(100, reducedMetrics.VisibleCount);
            Assert.AreEqual(100, fixture.Component.ActiveCount);
        }

        [Test]
        public void VfxVariants_ReportMappedOrDirectVisibleCounts()
        {
            using ModuleFixture<BatchedPulseVfxVariant> batched = CreateFixture<BatchedPulseVfxVariant>();
            using ModuleFixture<IndieEffectsVariant> indie = CreateFixture<IndieEffectsVariant>();

            batched.InvokeLifecycle("Awake");
            indie.InvokeLifecycle("Awake");

            batched.Component.SetStressLevel(4800);
            ShowcaseMetricsSnapshot batchedMetrics = batched.Component.GetMetricsSnapshot();
            ShowcaseMetricsSnapshot indieMetrics = indie.Component.GetMetricsSnapshot();

            Assert.AreEqual(4800, batchedMetrics.SimulationCount);
            Assert.AreEqual(240, batchedMetrics.VisibleCount);
            Assert.AreEqual(batched.Component.ActiveCount, batchedMetrics.VisibleCount);

            Assert.AreEqual(500, indieMetrics.SimulationCount);
            Assert.AreEqual(500, indieMetrics.VisibleCount);
            Assert.AreEqual(indie.Component.ActiveCount, indieMetrics.VisibleCount);
        }

        [Test]
        public void HumanoidAnimationVariant_FallbackRigReportsSimulationAndVisibleCounts()
        {
            using ModuleFixture<HumanoidAnimationVariant> fixture = CreateFixture<HumanoidAnimationVariant>();

            fixture.InvokeLifecycle("Awake");

            ShowcaseMetricsSnapshot metrics = fixture.Component.GetMetricsSnapshot();

            Assert.AreEqual(72, metrics.SimulationCount);
            Assert.AreEqual(42, metrics.VisibleCount);
            Assert.AreEqual(fixture.Component.ActiveCount, metrics.VisibleCount);
            Assert.AreEqual(42, fixture.Root.transform.childCount);
        }

        [Test]
        public void CreatureAnimationVariant_UsesVisibleCapForRigCount()
        {
            using ModuleFixture<CreatureAnimationVariant> fixture = CreateFixture<CreatureAnimationVariant>();

            fixture.InvokeLifecycle("Awake");

            ShowcaseMetricsSnapshot metrics = fixture.Component.GetMetricsSnapshot();

            Assert.AreEqual(64, metrics.SimulationCount);
            Assert.AreEqual(32, metrics.VisibleCount);
            Assert.AreEqual(fixture.Component.ActiveCount, metrics.VisibleCount);
            Assert.AreEqual(32, fixture.Root.transform.childCount);
        }

        private static ModuleFixture<T> CreateFixture<T>() where T : MonoBehaviour
        {
            GameObject root = new(typeof(T).Name + " Test Root");
            T component = root.AddComponent<T>();
            return new ModuleFixture<T>(root, component);
        }

        private sealed class ModuleFixture<T> : IDisposable where T : MonoBehaviour
        {
            private static readonly BindingFlags MethodFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            public ModuleFixture(GameObject root, T component)
            {
                Root = root;
                Component = component;
            }

            public GameObject Root { get; }
            public T Component { get; }

            public void InvokeLifecycle(string methodName)
            {
                MethodInfo method = Component.GetType().GetMethod(methodName, MethodFlags);
                if (method == null)
                    throw new MissingMethodException(Component.GetType().FullName, methodName);

                method.Invoke(Component, Array.Empty<object>());
            }

            public void Dispose()
            {
                if (Root != null)
                    UnityEngine.Object.DestroyImmediate(Root);
            }
        }
    }
}
