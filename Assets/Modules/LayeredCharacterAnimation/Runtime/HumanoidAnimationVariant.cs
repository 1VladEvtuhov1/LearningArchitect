using System;
using System.Diagnostics;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.Animation3D
{
    public sealed class HumanoidAnimationVariant : MonoBehaviour, IShowcaseStressTarget, IShowcaseMetricsSource
    {
        public enum PlaybackMode
        {
            Run = 0,
            Shoot = 1,
            RunAndShoot = 2
        }

        private struct HumanoidVisual
        {
            public Transform Root;
            public HumanoidCrowdActor Actor;
        }

        [Header("Crowd")]
        [SerializeField] private int count = 72;
        [SerializeField] private int visibleCount = 42;
        [SerializeField] private int visualLimit = 56;
        [SerializeField] private float radius = 6.2f;
        [SerializeField] private float moveSpeed = 1.25f;
        [SerializeField] private float cycleSpeed = 4f;
        [SerializeField] private float spawnPadding = 0.15f;

        [Header("Humanoid Profile")]
        [SerializeField] private HumanoidAnimationProfileSO animationProfile;

        [Header("Playback")]
        [SerializeField] private PlaybackMode playbackMode;
        [SerializeField] private bool translateActors = true;
        [SerializeField] private Vector3 presentationForward = Vector3.forward;

        private Vector3[] positions;
        private Vector3[] velocities;
        private float[] phases;
        private HumanoidVisual[] visuals;
        private float lastModuleCpuMs;

        public int ActiveCount => visuals == null ? 0 : visuals.Length;

        private void Awake()
        {
            EnsureAnimationProfileConfigured();
            Rebuild(count);
        }

        private void Update()
        {
            if (positions == null || velocities == null || phases == null)
                return;

            long startedAt = Stopwatch.GetTimestamp();
            float deltaTime = Time.deltaTime;
            float radiusSquared = radius * radius;
            bool usesLocomotion = playbackMode != PlaybackMode.Shoot;
            bool shouldMoveRoot = translateActors && usesLocomotion;
            for (int i = 0; i < count; i++)
            {
                if (shouldMoveRoot)
                {
                    Vector3 position = positions[i] + (velocities[i] * deltaTime);
                    if (position.sqrMagnitude > radiusSquared)
                    {
                        velocities[i] = -velocities[i];
                        position = positions[i] + (velocities[i] * deltaTime);
                    }

                    positions[i] = position;
                }

                phases[i] += deltaTime * cycleSpeed * (0.85f + ((i % 5) * 0.06f));
            }

            for (int i = 0; i < visuals.Length; i++)
            {
                Vector3 animationVelocity = usesLocomotion ? GetAnimationVelocity(i) : Vector3.zero;
                ApplyVisual(visuals[i], positions[i], animationVelocity, phases[i], deltaTime);
            }

            lastModuleCpuMs = (float)((Stopwatch.GetTimestamp() - startedAt) * 1000d / Stopwatch.Frequency);
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

        public ShowcaseMetricsSnapshot GetMetricsSnapshot()
        {
            int simulationCount = positions == null ? count : positions.Length;
            int visibleActors = visuals == null ? 0 : visuals.Length;
            return new ShowcaseMetricsSnapshot(simulationCount, visibleActors, lastModuleCpuMs);
        }

        private void Rebuild(int targetCount)
        {
            EnsureAnimationProfileConfigured();
            ClearVisuals();

            positions = new Vector3[targetCount];
            velocities = new Vector3[targetCount];
            phases = new float[targetCount];

            for (int i = 0; i < targetCount; i++)
            {
                positions[i] = translateActors
                    ? ShowcaseSpawnLayout.RandomPointOnPlatform(radius, spawnPadding)
                    : GetStationaryPresentationPosition(i);
                velocities[i] = translateActors
                    ? ShowcaseSpawnLayout.RandomVelocity(moveSpeed, 0.05f)
                    : GetPresentationForward() * moveSpeed;
                positions[i].y = 0f;
                velocities[i].y = 0f;
                phases[i] = i * 0.37f;
            }

            int visible = Mathf.Min(targetCount, Mathf.Min(visibleCount, visualLimit));
            visuals = new HumanoidVisual[visible];
            for (int i = 0; i < visible; i++)
                visuals[i] = CreateVisual(i);
        }

        private HumanoidVisual CreateVisual(int index)
        {
            return CreateActorVisual(index);
        }

        private HumanoidVisual CreateActorVisual(int index)
        {
            GameObject instance = Instantiate(animationProfile.ActorPrefab, transform);
            instance.name = animationProfile.ActorPrefab.name + " " + index;

            HumanoidCrowdActor actor = instance.GetComponent<HumanoidCrowdActor>();
            if (actor == null)
                actor = instance.AddComponent<HumanoidCrowdActor>();

            actor.Initialize(animationProfile, phases[index]);
            return new HumanoidVisual
            {
                Root = actor.ActorRoot,
                Actor = actor
            };
        }

        private void ApplyVisual(HumanoidVisual visual, Vector3 position, Vector3 velocity, float phase, float deltaTime)
        {
            if (visual.Actor != null)
                visual.Actor.ApplyFrame(animationProfile, position, velocity, deltaTime, IsShootingMode());
        }

        private bool IsShootingMode()
        {
            return playbackMode == PlaybackMode.RunAndShoot;
        }

        private Vector3 GetAnimationVelocity(int index)
        {
            if (translateActors)
                return velocities[index];

            return GetPresentationForward() * moveSpeed;
        }

        private Vector3 GetPresentationForward()
        {
            Vector3 forward = presentationForward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        }

        private Vector3 GetStationaryPresentationPosition(int index)
        {
            if (index == 0)
                return Vector3.zero;

            int ringIndex = index - 1;
            int side = Mathf.CeilToInt(Mathf.Sqrt(index + 1));
            int row = ringIndex / side;
            int column = ringIndex % side;
            float spacing = 1.15f;
            float x = (column - ((side - 1) * 0.5f)) * spacing;
            float z = 1.35f + row * spacing;
            return new Vector3(x, 0f, z);
        }

        private void EnsureAnimationProfileConfigured()
        {
            if (animationProfile == null)
                throw new InvalidOperationException($"{nameof(HumanoidAnimationVariant)} requires {nameof(animationProfile)}.");

            if (!animationProfile.HasActorPrefab)
                throw new InvalidOperationException($"{nameof(HumanoidAnimationVariant)} requires an actor prefab on {animationProfile.name}.");

            if (!animationProfile.HasAvatar)
                throw new InvalidOperationException($"{nameof(HumanoidAnimationVariant)} requires an avatar on {animationProfile.name}.");

            if (!animationProfile.HasAnimatorController)
                throw new InvalidOperationException($"{nameof(HumanoidAnimationVariant)} requires an animator controller on {animationProfile.name}.");

            if (!animationProfile.ActorPrefabHasAnimator)
                throw new InvalidOperationException(
                    $"{nameof(HumanoidAnimationVariant)} requires an Animator on actor prefab '{animationProfile.ActorPrefab.name}'.");
        }

        private void ClearVisuals()
        {
            if (visuals == null)
                return;

            for (int i = 0; i < visuals.Length; i++)
            {
                if (visuals[i].Root != null)
                    DestroyVisual(visuals[i].Root.gameObject);
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
