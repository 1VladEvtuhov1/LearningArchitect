using System.Diagnostics;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.Effects
{
    public sealed class ChunkEffectsVariant : MonoBehaviour, IShowcaseStressTarget, IShowcaseMetricsSource
    {
        [SerializeField] private int count = 5000;
        [SerializeField] private int visualCount = 240;
        [SerializeField] private int visualLimit = 420;
        [SerializeField] [Range(0.01f, 1f)] private float visibleFraction = 0.12f;
        [SerializeField] private float radius = 7f;
        [SerializeField] private float speed = 0.8f;

        private Vector3[] positions;
        private Vector3[] velocities;
        private Transform[] visuals;
        private float moduleCpuMs;

        public int ActiveCount => visuals == null ? 0 : visuals.Length;

        private void Awake()
        {
            Rebuild(count);
        }

        private void Update()
        {
            if (positions == null || velocities == null || visuals == null)
                return;

            long startedAt = Stopwatch.GetTimestamp();
            float dt = Time.deltaTime;
            float radiusSquared = radius * radius;

            for (int i = 0; i < count; i++)
            {
                Vector3 position = positions[i] + velocities[i] * dt;

                if (position.sqrMagnitude > radiusSquared)
                {
                    velocities[i] = -velocities[i];
                    position = positions[i] + velocities[i] * dt;
                }

                positions[i] = position;
            }

            for (int i = 0; i < visuals.Length; i++)
            {
                visuals[i].localPosition = positions[i];
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

            if (count == value && positions != null && positions.Length == value)
                return;

            count = value;
            Rebuild(count);
        }

        private void Rebuild(int targetCount)
        {
            ClearVisuals();

            positions = new Vector3[targetCount];
            velocities = new Vector3[targetCount];

            int visible;
            if (targetCount <= visualLimit)
            {
                visible = targetCount;
            }
            else
            {
                int scaledVisible = Mathf.CeilToInt(targetCount * visibleFraction);
                visible = Mathf.Clamp(scaledVisible, visualCount, visualLimit);
            }

            visuals = new Transform[visible];

            for (int i = 0; i < targetCount; i++)
            {
                positions[i] = ShowcaseSpawnLayout.RandomPointOnPlatform(radius);
                velocities[i] = ShowcaseSpawnLayout.RandomVelocity(speed);
            }

            for (int i = 0; i < visible; i++)
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                LearningArchitect.Core.ShowcasePrimitiveMaterialUtility.Apply(marker);
                marker.name = "Chunk Effect Visual " + i;
                marker.transform.SetParent(transform, false);
                marker.transform.localPosition = positions[i];
                marker.transform.localScale = Vector3.one * 0.12f;
                visuals[i] = marker.transform;
            }
        }

        private void ClearVisuals()
        {
            if (visuals == null)
                return;

            for (int i = 0; i < visuals.Length; i++)
            {
                Transform visual = visuals[i];
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
