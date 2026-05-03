using System;
using UnityEngine;

namespace LearningArchitect.Modules.AI
{
    public sealed class AiViewPresenter : IDisposable
    {
        private Transform parent;
        private PrimitiveType primitiveType;
        private Vector3 visualScale;
        private string visualNamePrefix;
        private bool orientToTarget;
        private int requestedVisibleCount;
        private int visualLimit;
        private Transform[] visuals;
        private bool configured;

        public int VisibleCount => visuals == null ? 0 : visuals.Length;

        public void Configure(
            Transform visualParent,
            PrimitiveType visualPrimitiveType,
            Vector3 scale,
            string namePrefix,
            bool shouldOrientToTarget,
            int visibleCount,
            int maxVisuals)
        {
            parent = visualParent != null ? visualParent : throw new ArgumentNullException(nameof(visualParent));
            primitiveType = visualPrimitiveType;
            visualScale = scale;
            visualNamePrefix = string.IsNullOrWhiteSpace(namePrefix)
                ? throw new ArgumentException("Visual name prefix is required.", nameof(namePrefix))
                : namePrefix;
            orientToTarget = shouldOrientToTarget;
            requestedVisibleCount = visibleCount >= 0 ? visibleCount : throw new ArgumentOutOfRangeException(nameof(visibleCount));
            visualLimit = maxVisuals >= 0 ? maxVisuals : throw new ArgumentOutOfRangeException(nameof(maxVisuals));
            configured = true;
        }

        public void Rebuild(AiWorld world)
        {
            EnsureConfigured();

            if (world == null)
                throw new ArgumentNullException(nameof(world));

            DestroyVisuals();

            int visible = Mathf.Min(world.Count, Mathf.Min(requestedVisibleCount, visualLimit));
            visuals = new Transform[visible];

            for (int i = 0; i < visible; i++)
            {
                GameObject marker = GameObject.CreatePrimitive(primitiveType);
                LearningArchitect.Core.ShowcasePrimitiveMaterialUtility.Apply(marker);
                marker.name = visualNamePrefix + i;
                marker.transform.SetParent(parent, false);
                marker.transform.localPosition = world.Positions[i];
                marker.transform.localScale = visualScale;

                Collider collider = marker.GetComponent<Collider>();
                if (collider != null)
                    DestroyObject(collider);

                visuals[i] = marker.transform;
            }
        }

        public void Sync(AiWorld world)
        {
            EnsureConfigured();

            if (world == null)
                throw new ArgumentNullException(nameof(world));
            if (visuals == null)
                throw new InvalidOperationException($"{nameof(AiViewPresenter)} visuals are not built.");

            for (int i = 0; i < visuals.Length; i++)
            {
                Transform visual = visuals[i];
                visual.localPosition = world.Positions[i];

                if (!orientToTarget)
                    continue;

                Vector3 direction = world.Targets[i] - world.Positions[i];
                visual.up = direction.sqrMagnitude > 0.0001f
                    ? direction.normalized
                    : Vector3.up;
            }
        }

        public void Dispose()
        {
            DestroyVisuals();
        }

        private void EnsureConfigured()
        {
            if (!configured)
                throw new InvalidOperationException($"{nameof(AiViewPresenter)} is not configured.");
        }

        private void DestroyVisuals()
        {
            if (visuals == null)
                return;

            for (int i = 0; i < visuals.Length; i++)
            {
                Transform visual = visuals[i];
                if (visual != null)
                    DestroyObject(visual.gameObject);
            }

            visuals = null;
        }

        private static void DestroyObject(UnityEngine.Object target)
        {
            if (target == null)
                return;

#if UNITY_EDITOR
            UnityEngine.Object.DestroyImmediate(target);
#else
            UnityEngine.Object.Destroy(target);
#endif
        }
    }
}
