using System;
using UnityEngine;

namespace LearningArchitect.Core
{
    public readonly struct ActiveVariantHandle
    {
        public readonly GameObject Instance;
        public readonly IModule Module;
        public readonly IShowcaseStressTarget StressTarget;
        public readonly IShowcaseMetricsSource MetricsSource;

        public ActiveVariantHandle(GameObject instance, IModule module, IShowcaseStressTarget stressTarget, IShowcaseMetricsSource metricsSource)
        {
            Instance = instance;
            Module = module;
            StressTarget = stressTarget;
            MetricsSource = metricsSource;
        }
    }

    public sealed class VariantSpawner
    {
        private readonly Transform moduleRoot;
        private GameObject activeInstance;

        public VariantSpawner(Transform moduleRoot)
        {
            this.moduleRoot = moduleRoot ?? throw new ArgumentNullException(nameof(moduleRoot));
        }

        public ActiveVariantHandle Spawn(VariantDefinitionSO variant)
        {
            if (variant == null)
                throw new ArgumentNullException(nameof(variant));

            if (variant.Prefab == null)
                throw new InvalidOperationException($"{nameof(VariantDefinitionSO)} '{variant.name}' requires a prefab.");

            activeInstance = UnityEngine.Object.Instantiate(variant.Prefab, moduleRoot);
            activeInstance.name = variant.Prefab.name;

            return new ActiveVariantHandle(
                activeInstance,
                activeInstance.GetComponent<IModule>(),
                activeInstance.GetComponent<IShowcaseStressTarget>(),
                activeInstance.GetComponent<IShowcaseMetricsSource>());
        }

        public void Despawn()
        {
            if (activeInstance == null)
                return;

            DestroyInstance(activeInstance);
            activeInstance = null;
        }

        private static void DestroyInstance(GameObject instance)
        {
            if (instance == null)
                return;

#if UNITY_EDITOR
            UnityEngine.Object.DestroyImmediate(instance);
#else
            UnityEngine.Object.Destroy(instance);
#endif
        }
    }
}
