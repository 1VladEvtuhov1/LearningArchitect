using System.Collections.Generic;
using System.Diagnostics;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.Effects
{
    public sealed class IndieEffectsVariant : MonoBehaviour, IShowcaseStressTarget, IShowcaseMetricsSource
    {
        [SerializeField] private GameObject effectPrefab;
        [SerializeField] private int count = 500;
        [SerializeField] private float radius = 6f;
        [SerializeField] private float rotateSpeed = 40f;

        private readonly List<GameObject> effects = new List<GameObject>(512);
        private int operationsPerFrame;
        private float simulationTimeMs;

        public int ActiveCount => effects.Count;

        private void Awake()
        {
            Rebuild(count);
        }

        private void Update()
        {
            long startedAt = Stopwatch.GetTimestamp();
            float angle = rotateSpeed * Time.deltaTime;

            for (int i = 0; i < effects.Count; i++)
            {
                GameObject effect = effects[i];
                if (effect != null)
                {
                    effect.transform.Rotate(Vector3.up, angle, Space.World);
                }
            }

            operationsPerFrame = effects.Count;
            simulationTimeMs = (float)((Stopwatch.GetTimestamp() - startedAt) * 1000d / Stopwatch.Frequency);
        }

        public ShowcaseMetricsSnapshot GetMetricsSnapshot()
        {
            return new ShowcaseMetricsSnapshot(count, ActiveCount, operationsPerFrame, simulationTimeMs);
        }

        public void SetStressLevel(int value)
        {
            if (value < 1)
                throw new System.ArgumentOutOfRangeException(nameof(value));

            if (count == value && effects.Count == value)
                return;

            count = value;
            Rebuild(count);
        }

        private void Rebuild(int targetCount)
        {
            ClearEffects();

            if (effects.Capacity < targetCount)
                effects.Capacity = targetCount;

            for (int i = 0; i < targetCount; i++)
            {
                Vector3 position = ShowcaseSpawnLayout.RandomPointOnPlatform(radius, 0.3f);

                GameObject instance = effectPrefab == null
                    ? GameObject.CreatePrimitive(PrimitiveType.Sphere)
                    : Instantiate(effectPrefab);

                if (effectPrefab == null)
                    LearningArchitect.Core.ShowcasePrimitiveMaterialUtility.Apply(instance);

                instance.name = "Indie Effect " + i;
                instance.transform.SetParent(transform, false);
                instance.transform.localPosition = position;
                instance.transform.localScale = Vector3.one * 0.14f;
                effects.Add(instance);
            }
        }

        private void ClearEffects()
        {
            for (int i = 0; i < effects.Count; i++)
            {
                GameObject effect = effects[i];
                if (effect != null)
                {
                    DestroyEffect(effect);
                }
            }

            effects.Clear();
        }

        private static void DestroyEffect(GameObject effect)
        {
#if UNITY_EDITOR
            DestroyImmediate(effect);
#else
            Destroy(effect);
#endif
        }
    }
}
