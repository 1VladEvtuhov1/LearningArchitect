using System;
using System.Diagnostics;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.VFX
{
    public sealed class BatchedPulseVfxVariant : MonoBehaviour, IShowcaseStressTarget, IShowcaseMetricsSource
    {
        [SerializeField] private int count = 3200;
        [SerializeField] private int visualLimit = 260;
        [SerializeField] private float radius = 6.8f;
        [SerializeField] private float pulseLifetime = 1.05f;
        [SerializeField] private float driftSpeed = 0.42f;
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] private Vector3 visualScale = Vector3.one * 0.16f;

        private MaterialPropertyBlock propertyBlock;

        private Vector3[] positions;
        private Vector3[] velocities;
        private float[] lifetimes;
        private float[] amplitudes;
        private Transform[] visuals;
        private Renderer[] renderers;
        private float moduleCpuMs;

        public int ActiveCount => visuals == null ? 0 : visuals.Length;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            Rebuild(count);
        }

        private void Update()
        {
            if (positions == null || lifetimes == null)
                return;

            long startedAt = Stopwatch.GetTimestamp();
            float deltaTime = Time.deltaTime;
            float radiusSquared = radius * radius;
            for (int i = 0; i < count; i++)
            {
                lifetimes[i] -= deltaTime;
                if (lifetimes[i] <= 0f)
                {
                    RespawnPulse(i);
                    continue;
                }

                Vector3 nextPosition = positions[i] + (velocities[i] * deltaTime);
                Vector2 planar = new Vector2(nextPosition.x, nextPosition.z);
                if (planar.sqrMagnitude > radiusSquared)
                {
                    velocities[i] = -velocities[i];
                    nextPosition = positions[i] + (velocities[i] * deltaTime);
                }

                positions[i] = ShowcaseSpawnLayout.ClampToSurface(nextPosition);
            }

            for (int i = 0; i < visuals.Length; i++)
                ApplyVisual(i);

            moduleCpuMs = (float)((Stopwatch.GetTimestamp() - startedAt) * 1000d / Stopwatch.Frequency);
        }

        public ShowcaseMetricsSnapshot GetMetricsSnapshot()
        {
            return new ShowcaseMetricsSnapshot(count, ActiveCount, moduleCpuMs);
        }

        public void SetStressLevel(int value)
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (count == value && positions != null && positions.Length == value)
                return;

            count = value;
            Rebuild(count);
        }

        private void Rebuild(int targetCount)
        {
            ClearVisuals();

            int spawnCount = ShowcaseStressSpawn.Clamp(targetCount, visualLimit);
            count = spawnCount;
            positions = new Vector3[spawnCount];
            velocities = new Vector3[spawnCount];
            lifetimes = new float[spawnCount];
            amplitudes = new float[spawnCount];

            for (int i = 0; i < spawnCount; i++)
                RespawnPulse(i);

            visuals = new Transform[spawnCount];
            renderers = new Renderer[spawnCount];
            for (int i = 0; i < spawnCount; i++)
            {
                GameObject marker = ShowcaseVisualInstanceFactory.CreateMarker(
                    transform,
                    "Batched Pulse " + i,
                    visualPrefab,
                    visualScale);
                visuals[i] = marker.transform;
                renderers[i] = ShowcaseVisualInstanceFactory.FindPrimaryRenderer(marker);
                ApplyVisual(i);
            }
        }

        private void RespawnPulse(int index)
        {
            positions[index] = ShowcaseSpawnLayout.RandomPointOnPlatform(radius, 0.22f);
            velocities[index] = ShowcaseSpawnLayout.RandomVelocity(driftSpeed, 0.25f);
            lifetimes[index] = pulseLifetime * (0.55f + ((index % 9) * 0.08f));
            amplitudes[index] = 0.16f + ((index % 7) * 0.04f);
        }

        private void ApplyVisual(int visualIndex)
        {
            int pulseIndex = visualIndex % count;
            float normalizedLife = Mathf.Clamp01(lifetimes[pulseIndex] / pulseLifetime);
            float scale = Mathf.Lerp(0.04f, amplitudes[pulseIndex], normalizedLife);
            Color color = Color.Lerp(
                new Color(0.22f, 0.42f, 0.98f, 1f),
                new Color(1f, 0.32f, 0.76f, 1f),
                1f - normalizedLife);

            Transform visual = visuals[visualIndex];
            visual.localPosition = positions[pulseIndex];
            visual.localScale = Vector3.one * scale;

            propertyBlock.Clear();
            propertyBlock.SetColor("_BaseColor", color);
            propertyBlock.SetColor("_Color", color);
            renderers[visualIndex].SetPropertyBlock(propertyBlock);
        }

        private void ClearVisuals()
        {
            if (visuals == null)
                return;

            for (int i = 0; i < visuals.Length; i++)
            {
                if (visuals[i] != null)
                    DestroyVisual(visuals[i].gameObject);
            }
        }

        private static void DestroyVisual(GameObject visual)
        {
#if UNITY_EDITOR
            DestroyImmediate(visual);
#else
            Destroy(visual);
#endif
        }
    }
}
