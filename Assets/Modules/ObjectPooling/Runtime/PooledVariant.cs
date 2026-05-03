using System;
using System.Diagnostics;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.Pooling
{
    public sealed class PooledVariant : MonoBehaviour, IShowcaseStressTarget, IShowcaseMetricsSource
    {
        private struct Projectile
        {
            public Transform Transform;
            public Vector3 Velocity;
            public float Lifetime;
            public bool Active;
        }

        [SerializeField] private int count = 5000;
        [SerializeField] private int visualLimit = 240;
        [SerializeField] private float spawnRadius = 1.2f;
        [SerializeField] private float projectileSpeed = 4.5f;
        [SerializeField] private float projectileLifetime = 1.8f;

        private Projectile[] projectiles;
        private int targetVisualCount;
        private int nextSpawnIndex;
        private int operationsPerFrame;
        private float simulationTimeMs;

        public int ActiveCount
        {
            get
            {
                if (projectiles == null)
                    return 0;

                int active = 0;
                for (int i = 0; i < projectiles.Length; i++)
                {
                    if (projectiles[i].Active)
                        active++;
                }

                return active;
            }
        }

        private void Awake()
        {
            BuildPool();
            ApplyStressLevel(count);
        }

        private void Update()
        {
            long startedAt = Stopwatch.GetTimestamp();
            int activationsThisFrame = EnsureActiveProjectiles();

            for (int i = 0; i < projectiles.Length; i++)
            {
                Projectile projectile = projectiles[i];
                if (!projectile.Active)
                    continue;

                projectile.Lifetime -= Time.deltaTime;
                if (projectile.Lifetime <= 0f)
                {
                    projectile.Active = false;
                    projectile.Transform.gameObject.SetActive(false);
                    projectiles[i] = projectile;
                    continue;
                }

                projectile.Transform.localPosition += projectile.Velocity * Time.deltaTime;
                projectiles[i] = projectile;
            }

            operationsPerFrame = ActiveCount + activationsThisFrame;
            simulationTimeMs = (float)((Stopwatch.GetTimestamp() - startedAt) * 1000d / Stopwatch.Frequency);
        }

        public ShowcaseMetricsSnapshot GetMetricsSnapshot()
        {
            return new ShowcaseMetricsSnapshot(targetVisualCount, ActiveCount, operationsPerFrame, simulationTimeMs);
        }

        public void SetStressLevel(int value)
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value));

            count = value;
            ApplyStressLevel(value);
        }

        private void BuildPool()
        {
            ClearPool();

            projectiles = new Projectile[visualLimit];
            for (int i = 0; i < visualLimit; i++)
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                LearningArchitect.Core.ShowcasePrimitiveMaterialUtility.Apply(marker);
                marker.name = "Pooled Projectile " + i;
                marker.transform.SetParent(transform, false);
                marker.transform.localScale = Vector3.one * 0.12f;
                marker.SetActive(false);
                projectiles[i] = new Projectile
                {
                    Transform = marker.transform,
                    Velocity = Vector3.zero,
                    Lifetime = 0f,
                    Active = false
                };
            }
        }

        private void ApplyStressLevel(int value)
        {
            targetVisualCount = Mathf.Clamp(value / 20, 24, visualLimit);
            nextSpawnIndex = 0;

            for (int i = 0; i < projectiles.Length; i++)
            {
                Projectile projectile = projectiles[i];
                if (!projectile.Active)
                    continue;

                projectile.Active = false;
                projectile.Transform.gameObject.SetActive(false);
                projectiles[i] = projectile;
            }
        }

        private int EnsureActiveProjectiles()
        {
            int activeVisualCount = 0;
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].Active)
                    activeVisualCount++;
            }

            int activations = 0;
            while (activeVisualCount < targetVisualCount)
            {
                ActivateProjectile(nextSpawnIndex % projectiles.Length);
                nextSpawnIndex++;
                activeVisualCount++;
                activations++;
            }

            return activations;
        }

        private void ActivateProjectile(int index)
        {
            Projectile projectile = projectiles[index];
            projectile.Transform.localPosition = ShowcaseSpawnLayout.RandomPointOnPlatform(spawnRadius, 0.25f);
            projectile.Transform.gameObject.SetActive(true);

            Vector3 velocity = ShowcaseSpawnLayout.RandomVelocity(projectileSpeed, 0.25f);

            projectile.Velocity = velocity;
            projectile.Lifetime = projectileLifetime;
            projectile.Active = true;
            projectiles[index] = projectile;
        }

        private void ClearPool()
        {
            if (projectiles == null)
                return;

            for (int i = 0; i < projectiles.Length; i++)
            {
                Transform visual = projectiles[i].Transform;
                if (visual != null)
                    DestroyVisual(visual.gameObject);
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
