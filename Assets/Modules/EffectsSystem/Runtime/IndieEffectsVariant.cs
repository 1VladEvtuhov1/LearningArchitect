using System.Collections.Generic;
using System.Diagnostics;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.Effects
{
    public sealed class IndieEffectsVariant : MonoBehaviour, IShowcaseStressTarget, IShowcaseMetricsSource
    {
        [SerializeField] private GameObject effectPrefab;
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] private int count = 500;
        [SerializeField] private float radius = 6f;
        [SerializeField] private float rotateSpeed = 40f;
        [SerializeField] private Vector3 visualScale = Vector3.one * 0.14f;

        private readonly List<GameObject> effects = new List<GameObject>(512);
        private float moduleCpuMs;

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

            moduleCpuMs = (float)((Stopwatch.GetTimestamp() - startedAt) * 1000d / Stopwatch.Frequency);
        }

        public ShowcaseMetricsSnapshot GetMetricsSnapshot()
        {
            return new ShowcaseMetricsSnapshot(count, ActiveCount, moduleCpuMs);
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
                    ? ShowcaseVisualInstanceFactory.CreateMarker(
                        transform,
                        "Indie Effect " + i,
                        visualPrefab,
                        visualScale)
                    : Instantiate(effectPrefab, transform, false);

                instance.name = "Indie Effect " + i;
                instance.transform.localPosition = position;
                if (effectPrefab != null)
                    instance.transform.localScale = visualScale;
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
