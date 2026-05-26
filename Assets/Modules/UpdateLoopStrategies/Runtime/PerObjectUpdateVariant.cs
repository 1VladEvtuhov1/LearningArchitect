using System;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.Performance
{
    public sealed class PerObjectUpdateVariant : MonoBehaviour, IShowcaseStressTarget, IShowcaseMetricsSource
    {
        [SerializeField] private int count = 5000;
        [SerializeField] private int visualLimit = 420;
        [SerializeField] private float radius = 7f;
        [SerializeField] private float speed = 0.85f;
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] private Vector3 visualScale = Vector3.one * 0.1f;

        private Transform[] visuals;

        public int ActiveCount => visuals == null ? 0 : visuals.Length;

        private void Awake()
        {
            PerObjectUpdateMover.ResetMetrics();
            Rebuild(count);
        }

        public ShowcaseMetricsSnapshot GetMetricsSnapshot()
        {
            return new ShowcaseMetricsSnapshot(
                count,
                ActiveCount,
                PerObjectUpdateMover.GetLastModuleCpuMs());
        }

        public void SetStressLevel(int value)
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (count == value)
                return;

            count = value;
            Rebuild(count);
        }

        private void Rebuild(int targetCount)
        {
            ClearVisuals();
            PerObjectUpdateMover.ResetMetrics();

            int spawnCount = ShowcaseStressSpawn.Clamp(targetCount, visualLimit);
            count = spawnCount;
            visuals = new Transform[spawnCount];

            for (int i = 0; i < spawnCount; i++)
            {
                GameObject marker = ShowcaseVisualInstanceFactory.CreateMarker(
                    transform,
                    "PerObject Mover " + i,
                    visualPrefab,
                    visualScale);
                marker.transform.localPosition = ShowcaseSpawnLayout.RandomPointOnPlatform(radius);

                PerObjectUpdateMover mover = marker.AddComponent<PerObjectUpdateMover>();
                mover.Configure(radius, speed);

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
