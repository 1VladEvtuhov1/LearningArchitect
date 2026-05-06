using System;
using System.Collections.Generic;
using System.Diagnostics;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.Pooling
{
    public sealed class InstantiationVariant : MonoBehaviour, IShowcaseStressTarget, IShowcaseMetricsSource
    {
        private struct Projectile
        {
            public Transform Transform;
            public Vector3 Velocity;
            public float Lifetime;
        }

        [SerializeField] private int count = 5000;
        [SerializeField] private int visualLimit = 240;
        [SerializeField] private float spawnRadius = 1.2f;
        [SerializeField] private float projectileSpeed = 4.5f;
        [SerializeField] private float projectileLifetime = 1.8f;
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] private Vector3 visualScale = Vector3.one * 0.12f;

        private readonly List<Projectile> activeProjectiles = new(256);
        private int targetVisualCount;
        private int nextSpawnIndex;
        private float moduleCpuMs;

        public int ActiveCount => activeProjectiles.Count;

        private void Awake()
        {
            ApplyStressLevel(count);
        }

        private void Update()
        {
            long startedAt = Stopwatch.GetTimestamp();
            while (activeProjectiles.Count < targetVisualCount)
                SpawnProjectile();

            for (int i = activeProjectiles.Count - 1; i >= 0; i--)
            {
                Projectile projectile = activeProjectiles[i];
                projectile.Lifetime -= Time.deltaTime;

                if (projectile.Lifetime <= 0f)
                {
                    DestroyVisual(projectile.Transform.gameObject);
                    activeProjectiles.RemoveAt(i);
                    continue;
                }

                projectile.Transform.localPosition += projectile.Velocity * Time.deltaTime;
                activeProjectiles[i] = projectile;
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
                throw new ArgumentOutOfRangeException(nameof(value));

            count = value;
            ApplyStressLevel(value);
        }

        private void ApplyStressLevel(int value)
        {
            targetVisualCount = Mathf.Clamp(value / 20, 24, visualLimit);
            nextSpawnIndex = 0;

            while (activeProjectiles.Count > targetVisualCount)
            {
                int lastIndex = activeProjectiles.Count - 1;
                DestroyVisual(activeProjectiles[lastIndex].Transform.gameObject);
                activeProjectiles.RemoveAt(lastIndex);
            }
        }

        private void SpawnProjectile()
        {
            GameObject marker = ShowcaseVisualInstanceFactory.CreateMarker(
                transform,
                "Instantiation Projectile " + nextSpawnIndex++,
                visualPrefab,
                visualScale);
            marker.transform.localPosition = ShowcaseSpawnLayout.RandomPointOnPlatform(spawnRadius, 0.25f);

            Vector3 velocity = ShowcaseSpawnLayout.RandomVelocity(projectileSpeed, 0.25f);

            activeProjectiles.Add(new Projectile
            {
                Transform = marker.transform,
                Velocity = velocity,
                Lifetime = projectileLifetime
            });
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
