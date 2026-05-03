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

        private struct HumanoidRig
        {
            public Transform Root;
            public Transform Body;
            public Transform Head;
            public Transform ArmLeft;
            public Transform ArmRight;
            public Transform LegLeft;
            public Transform LegRight;
        }

        private struct HumanoidVisual
        {
            public Transform Root;
            public HumanoidCrowdActor Actor;
            public HumanoidRig FallbackRig;
            public bool UsesActor;
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

        private MaterialPropertyBlock propertyBlock;

        private Vector3[] positions;
        private Vector3[] velocities;
        private float[] phases;
        private HumanoidVisual[] visuals;
        private float lastModuleCpuMs;

        public int ActiveCount => visuals == null ? 0 : visuals.Length;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
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
            if (animationProfile != null && animationProfile.HasActorPrefab)
                return CreateActorVisual(index);

            HumanoidRig fallbackRig = CreateFallbackRig(index);
            return new HumanoidVisual
            {
                Root = fallbackRig.Root,
                FallbackRig = fallbackRig,
                UsesActor = false
            };
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
                Actor = actor,
                UsesActor = true
            };
        }

        private HumanoidRig CreateFallbackRig(int index)
        {
            GameObject root = new("Humanoid Rig " + index);
            root.transform.SetParent(transform, false);

            Transform body = CreatePart(root.transform, PrimitiveType.Cube, "Body", new Vector3(0f, 0.55f, 0f), new Vector3(0.24f, 0.54f, 0.16f), new Color(0.27f, 0.68f, 0.92f));
            Transform head = CreatePart(root.transform, PrimitiveType.Sphere, "Head", new Vector3(0f, 0.98f, 0f), new Vector3(0.18f, 0.18f, 0.18f), new Color(0.92f, 0.8f, 0.62f));
            Transform armLeft = CreatePart(root.transform, PrimitiveType.Capsule, "ArmLeft", new Vector3(-0.18f, 0.62f, 0f), new Vector3(0.08f, 0.26f, 0.08f), new Color(0.17f, 0.48f, 0.82f));
            Transform armRight = CreatePart(root.transform, PrimitiveType.Capsule, "ArmRight", new Vector3(0.18f, 0.62f, 0f), new Vector3(0.08f, 0.26f, 0.08f), new Color(0.17f, 0.48f, 0.82f));
            Transform legLeft = CreatePart(root.transform, PrimitiveType.Capsule, "LegLeft", new Vector3(-0.08f, 0.2f, 0f), new Vector3(0.09f, 0.3f, 0.09f), new Color(0.16f, 0.2f, 0.28f));
            Transform legRight = CreatePart(root.transform, PrimitiveType.Capsule, "LegRight", new Vector3(0.08f, 0.2f, 0f), new Vector3(0.09f, 0.3f, 0.09f), new Color(0.16f, 0.2f, 0.28f));

            return new HumanoidRig
            {
                Root = root.transform,
                Body = body,
                Head = head,
                ArmLeft = armLeft,
                ArmRight = armRight,
                LegLeft = legLeft,
                LegRight = legRight
            };
        }

        private void ApplyVisual(HumanoidVisual visual, Vector3 position, Vector3 velocity, float phase, float deltaTime)
        {
            if (visual.UsesActor)
            {
                if (visual.Actor != null)
                    visual.Actor.ApplyFrame(animationProfile, position, velocity, deltaTime, IsShootingMode());

                return;
            }

            ApplyRigPose(visual.FallbackRig, position, velocity, phase);
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

        private void ApplyRigPose(HumanoidRig rig, Vector3 position, Vector3 velocity, float phase)
        {
            float bob = Mathf.Sin(phase * 2f) * 0.04f;
            float armSwing = Mathf.Sin(phase) * 38f;
            float legSwing = Mathf.Sin(phase + Mathf.PI) * 42f;

            rig.Root.localPosition = position;
            Vector3 flatVelocity = velocity;
            flatVelocity.y = 0f;
            if (flatVelocity.sqrMagnitude > 0.0001f)
                rig.Root.rotation = Quaternion.LookRotation(flatVelocity.normalized, Vector3.up);

            rig.Body.localPosition = new Vector3(0f, 0.55f + bob, 0f);
            rig.Head.localPosition = new Vector3(0f, 0.98f + bob * 0.45f, 0f);
            rig.Head.localRotation = Quaternion.Euler(Mathf.Sin(phase * 0.5f) * 8f, 0f, 0f);
            rig.ArmLeft.localRotation = Quaternion.Euler(armSwing, 0f, 8f);
            rig.ArmRight.localRotation = Quaternion.Euler(-armSwing, 0f, -8f);
            rig.LegLeft.localRotation = Quaternion.Euler(legSwing, 0f, 0f);
            rig.LegRight.localRotation = Quaternion.Euler(-legSwing, 0f, 0f);
        }

        private Transform CreatePart(Transform parent, PrimitiveType type, string name, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            LearningArchitect.Core.ShowcasePrimitiveMaterialUtility.Apply(part);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;

            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                propertyBlock.Clear();
                propertyBlock.SetColor("_BaseColor", color);
                propertyBlock.SetColor("_Color", color);
                renderer.SetPropertyBlock(propertyBlock);
            }

            return part.transform;
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
