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
        [SerializeField] private int count = 160;
        [SerializeField] private int visualLimit = 420;
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

            int spawnCount = ShowcaseStressSpawn.Clamp(value, visualLimit);
            if (count == spawnCount && effects.Count == spawnCount)
                return;

            count = spawnCount;
            Rebuild(spawnCount);
        }

        private void Rebuild(int targetCount)
        {
            ClearEffects();

            int spawnCount = ShowcaseStressSpawn.Clamp(targetCount, visualLimit);
            count = spawnCount;

            if (effects.Capacity < spawnCount)
                effects.Capacity = spawnCount;

            for (int i = 0; i < spawnCount; i++)
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
