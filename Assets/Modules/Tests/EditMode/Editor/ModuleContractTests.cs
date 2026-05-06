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
using UnityEditor;
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
            centralized.Component.SetStressLevel(5000);
            perObject.Component.SetStressLevel(5000);

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
            fixture.Component.SetStressLevel(5000);

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
            indie.Component.SetStressLevel(500);
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
        public void HumanoidAnimationVariant_ActorPrefabReportsSimulationAndVisibleCounts()
        {
            using ModuleFixture<HumanoidAnimationVariant> fixture = CreateFixture<HumanoidAnimationVariant>();

            fixture.InvokeLifecycle("Awake");
            fixture.Component.SetStressLevel(24);

            ShowcaseMetricsSnapshot metrics = fixture.Component.GetMetricsSnapshot();

            Assert.AreEqual(24, metrics.SimulationCount);
            Assert.AreEqual(8, metrics.VisibleCount);
            Assert.AreEqual(fixture.Component.ActiveCount, metrics.VisibleCount);
            Assert.AreEqual(8, fixture.Root.transform.childCount);
            Assert.IsNotNull(fixture.Root.transform.GetChild(0).GetComponent<HumanoidCrowdActor>());
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

        [TestCase("Assets/Modules/UpdateLoopStrategies/Prefabs/UpdateLoopVariant_PerObject.prefab", nameof(PerObjectUpdateVariant))]
        [TestCase("Assets/Modules/UpdateLoopStrategies/Prefabs/UpdateLoopVariant_Centralized.prefab", nameof(CentralizedUpdateVariant))]
        [TestCase("Assets/Modules/ObjectPooling/Prefabs/PoolingVariant_Instantiation.prefab", nameof(InstantiationVariant))]
        [TestCase("Assets/Modules/ObjectPooling/Prefabs/PoolingVariant_Pooled.prefab", nameof(PooledVariant))]
        [TestCase("Assets/Modules/InventorySystems/Prefabs/InventoryVariant_PackedSlots.prefab", nameof(InventoryPackedVariant))]
        [TestCase("Assets/Modules/InventorySystems/Prefabs/InventoryVariant_ObjectSlots.prefab", nameof(InventoryObjectsVariant))]
        [TestCase("Assets/Modules/AISystem/Prefabs/AiVariant_BehaviorTree.prefab", "BehaviorTreeAiVariant")]
        [TestCase("Assets/Modules/AISystem/Prefabs/AiVariant_FSM.prefab", "FsmAiVariant")]
        [TestCase("Assets/Modules/AISystem/Prefabs/AiVariant_Utility.prefab", "UtilityAiVariant")]
        [TestCase("Assets/Modules/EffectsSystem/Prefabs/EffectsVariant_Chunk.prefab", nameof(ChunkEffectsVariant))]
        [TestCase("Assets/Modules/EffectsSystem/Prefabs/EffectsVariant_Indie.prefab", nameof(IndieEffectsVariant))]
        [TestCase("Assets/Modules/VFXDelivery/Prefabs/VfxVariant_BatchedPulses.prefab", nameof(BatchedPulseVfxVariant))]
        public void ModulePrefabs_AssignVisualPrefabForRuntimeSpawnedVisuals(string prefabPath, string componentTypeName)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.IsNotNull(prefab, $"Could not load prefab at '{prefabPath}'.");

            MonoBehaviour component = Array.Find(prefab.GetComponents<MonoBehaviour>(), candidate =>
                candidate != null && candidate.GetType().Name == componentTypeName);
            Assert.IsNotNull(component, $"Prefab '{prefabPath}' is missing component '{componentTypeName}'.");

            SerializedObject serializedObject = new(component);
            SerializedProperty visualPrefabProperty = serializedObject.FindProperty("visualPrefab");
            Assert.IsNotNull(visualPrefabProperty, $"Component '{componentTypeName}' should expose a serialized 'visualPrefab' field.");
            Assert.IsNotNull(visualPrefabProperty.objectReferenceValue, $"Prefab '{prefabPath}' must assign a runtime visual prefab.");
        }

        private static ModuleFixture<T> CreateFixture<T>() where T : MonoBehaviour
        {
            string prefabPath = GetFixturePrefabPath(typeof(T));
            GameObject root;
            T component;

            if (!string.IsNullOrEmpty(prefabPath))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                Assert.IsNotNull(prefab, $"Could not load fixture prefab at '{prefabPath}' for '{typeof(T).Name}'.");
                root = UnityEngine.Object.Instantiate(prefab);
                component = root.GetComponent<T>();
                Assert.IsNotNull(component, $"Instantiated fixture prefab '{prefabPath}' is missing component '{typeof(T).Name}'.");
                ConfigureVisualPrefabIfPresent(component);
            }
            else
            {
                root = new(typeof(T).Name + " Test Root");
                component = root.AddComponent<T>();
                ConfigureVisualPrefabIfPresent(component);
            }

            return new ModuleFixture<T>(root, component);
        }

        private static string GetFixturePrefabPath(Type componentType)
        {
            return componentType.Name switch
            {
                nameof(PerObjectUpdateVariant) => "Assets/Modules/UpdateLoopStrategies/Prefabs/UpdateLoopVariant_PerObject.prefab",
                nameof(CentralizedUpdateVariant) => "Assets/Modules/UpdateLoopStrategies/Prefabs/UpdateLoopVariant_Centralized.prefab",
                nameof(InstantiationVariant) => "Assets/Modules/ObjectPooling/Prefabs/PoolingVariant_Instantiation.prefab",
                nameof(PooledVariant) => "Assets/Modules/ObjectPooling/Prefabs/PoolingVariant_Pooled.prefab",
                nameof(InventoryPackedVariant) => "Assets/Modules/InventorySystems/Prefabs/InventoryVariant_PackedSlots.prefab",
                nameof(InventoryObjectsVariant) => "Assets/Modules/InventorySystems/Prefabs/InventoryVariant_ObjectSlots.prefab",
                nameof(ChunkEffectsVariant) => "Assets/Modules/EffectsSystem/Prefabs/EffectsVariant_Chunk.prefab",
                nameof(IndieEffectsVariant) => "Assets/Modules/EffectsSystem/Prefabs/EffectsVariant_Indie.prefab",
                nameof(BatchedPulseVfxVariant) => "Assets/Modules/VFXDelivery/Prefabs/VfxVariant_BatchedPulses.prefab",
                nameof(HumanoidAnimationVariant) => "Assets/Modules/LayeredCharacterAnimation/Prefabs/AnimationVariant_Run.prefab",
                _ => null
            };
        }

        private static void ConfigureVisualPrefabIfPresent(MonoBehaviour component)
        {
            SerializedObject serializedObject = new(component);
            SerializedProperty visualPrefabProperty = serializedObject.FindProperty("visualPrefab");
            if (visualPrefabProperty == null || visualPrefabProperty.propertyType != SerializedPropertyType.ObjectReference)
                return;

            string prefabPath = component.GetType().Name switch
            {
                nameof(PerObjectUpdateVariant) => "Assets/Showcase/Art/ModuleCarriers/PerObjectCarrier.prefab",
                nameof(CentralizedUpdateVariant) => "Assets/Showcase/Art/ModuleCarriers/CentralizedCarrier.prefab",
                nameof(InstantiationVariant) => "Assets/Showcase/Art/ModuleCarriers/ProjectileCarrier.prefab",
                nameof(PooledVariant) => "Assets/Showcase/Art/ModuleCarriers/ProjectileCarrier.prefab",
                nameof(InventoryPackedVariant) => "Assets/Showcase/Art/ModuleCarriers/InventoryCellCarrier.prefab",
                nameof(InventoryObjectsVariant) => "Assets/Showcase/Art/ModuleCarriers/InventoryCellCarrier.prefab",
                nameof(ChunkEffectsVariant) => "Assets/Showcase/Art/ModuleCarriers/ChunkCarrier.prefab",
                nameof(IndieEffectsVariant) => "Assets/Showcase/Art/ModuleCarriers/PulseCarrier.prefab",
                nameof(BatchedPulseVfxVariant) => "Assets/Showcase/Art/ModuleCarriers/PulseCarrier.prefab",
                _ => null
            };

            if (string.IsNullOrEmpty(prefabPath))
                return;

            GameObject visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.IsNotNull(visualPrefab, $"Required visual prefab at '{prefabPath}' could not be loaded for test fixture '{component.GetType().Name}'.");
            visualPrefabProperty.objectReferenceValue = visualPrefab;

            SerializedProperty visualScaleProperty = serializedObject.FindProperty("visualScale");
            if (visualScaleProperty != null && visualScaleProperty.propertyType == SerializedPropertyType.Vector3)
                visualScaleProperty.vector3Value = Vector3.one;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
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
