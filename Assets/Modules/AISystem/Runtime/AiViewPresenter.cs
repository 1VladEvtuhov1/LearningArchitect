using System;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.AI
{
    public sealed class AiViewPresenter : IDisposable
    {
        private Transform parent;
        private GameObject visualPrefab;
        private Vector3 visualScale;
        private string visualNamePrefix;
        private bool orientToTarget;
        private int requestedVisibleCount;
        private int visualLimit;
        private Transform[] visuals;
        private Vector3[] previousPositions;
        private bool configured;

        private const float MinFacingDistance = 0.2f;
        private const float MinFacingDistanceSquared = MinFacingDistance * MinFacingDistance;
        private const float MinMovementFacingSquared = 0.0025f;
        private const float RotationLerpSpeed = 14f;

        public int VisibleCount => visuals == null ? 0 : visuals.Length;

        public void Configure(
            Transform visualParent,
            GameObject markerPrefab,
            Vector3 scale,
            string namePrefix,
            bool shouldOrientToTarget,
            int visibleCount,
            int maxVisuals)
        {
            parent = visualParent != null ? visualParent : throw new ArgumentNullException(nameof(visualParent));
            visualPrefab = markerPrefab;
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
            previousPositions = new Vector3[visible];

            for (int i = 0; i < visible; i++)
            {
                GameObject marker = ShowcaseVisualInstanceFactory.CreateMarker(
                    parent,
                    visualNamePrefix + i,
                    visualPrefab,
                    visualScale);
                marker.transform.localPosition = world.Positions[i];
                previousPositions[i] = world.Positions[i];
                ApplyOrientation(marker.transform, world.Positions[i], world.Targets[i], i, 0f);
                visuals[i] = marker.transform;
            }
        }

        public void Sync(AiWorld world, float deltaTime)
        {
            EnsureConfigured();

            if (world == null)
                throw new ArgumentNullException(nameof(world));
            if (visuals == null || previousPositions == null)
                throw new InvalidOperationException($"{nameof(AiViewPresenter)} visuals are not built.");

            for (int i = 0; i < visuals.Length; i++)
            {
                Transform visual = visuals[i];
                visual.localPosition = world.Positions[i];
                ApplyOrientation(visual, world.Positions[i], world.Targets[i], i, deltaTime);
            }
        }

        private void ApplyOrientation(Transform visual, Vector3 position, Vector3 target, int agentIndex, float deltaTime)
        {
            if (!orientToTarget)
                return;

            Vector3 movement = position - previousPositions[agentIndex];
            movement.y = 0f;

            Vector3 toTarget = target - position;
            toTarget.y = 0f;

            Vector3 faceDirection;
            if (movement.sqrMagnitude > MinMovementFacingSquared)
                faceDirection = movement;
            else if (toTarget.sqrMagnitude > MinFacingDistanceSquared)
                faceDirection = toTarget;
            else
            {
                previousPositions[agentIndex] = position;
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(faceDirection.normalized, Vector3.up);
            if (deltaTime <= 0f)
                visual.localRotation = targetRotation;
            else
            {
                float blend = 1f - Mathf.Exp(-RotationLerpSpeed * deltaTime);
                visual.localRotation = Quaternion.Slerp(visual.localRotation, targetRotation, blend);
            }

            previousPositions[agentIndex] = position;
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
            previousPositions = null;
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
