using System;
using System.Diagnostics;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.Animation3D
{
    public sealed class CreatureAnimationVariant : MonoBehaviour, IShowcaseStressTarget, IShowcaseMetricsSource
    {
        private struct CreatureRig
        {
            public Transform Root;
            public Transform Body;
            public Transform Head;
            public Transform Tail;
            public Transform LegFrontLeft;
            public Transform LegFrontRight;
            public Transform LegBackLeft;
            public Transform LegBackRight;
        }

        [SerializeField] private int count = 64;
        [SerializeField] private int visibleCount = 32;
        [SerializeField] private int visualLimit = 44;
        [SerializeField] private float radius = 6.5f;
        [SerializeField] private float moveSpeed = 1.45f;
        [SerializeField] private float gaitSpeed = 4.6f;

        private MaterialPropertyBlock propertyBlock;

        private Vector3[] positions;
        private Vector3[] velocities;
        private float[] phases;
        private CreatureRig[] rigs;
        private float lastModuleCpuMs;

        public int ActiveCount => rigs == null ? 0 : rigs.Length;

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

            for (int i = 0; i < count; i++)
            {
                Vector3 position = positions[i] + (velocities[i] * deltaTime);
                if (position.sqrMagnitude > radiusSquared)
                {
                    velocities[i] = -velocities[i];
                    position = positions[i] + (velocities[i] * deltaTime);
                }

                position.y = 0f;
                velocities[i].y = 0f;
                positions[i] = position;
                phases[i] += deltaTime * gaitSpeed * (0.9f + ((i % 4) * 0.08f));
            }

            for (int i = 0; i < rigs.Length; i++)
                ApplyRigPose(rigs[i], positions[i], velocities[i], phases[i]);

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
            int visibleActors = rigs == null ? 0 : rigs.Length;
            return new ShowcaseMetricsSnapshot(simulationCount, visibleActors, lastModuleCpuMs);
        }

        private void Rebuild(int targetCount)
        {
            ClearRigs();

            positions = new Vector3[targetCount];
            velocities = new Vector3[targetCount];
            phases = new float[targetCount];

            for (int i = 0; i < targetCount; i++)
            {
                positions[i] = ShowcaseSpawnLayout.RandomPointOnPlatform(radius, 0.12f);
                positions[i].y = 0f;
                velocities[i] = ShowcaseSpawnLayout.RandomVelocity(moveSpeed, 0.04f);
                velocities[i].y = 0f;
                phases[i] = (i * 0.52f) + 0.7f;
            }

            int visible = Mathf.Min(targetCount, Mathf.Min(visibleCount, visualLimit));
            rigs = new CreatureRig[visible];
            for (int i = 0; i < visible; i++)
                rigs[i] = CreateRig(i);
        }

        private CreatureRig CreateRig(int index)
        {
            GameObject root = new("Creature Rig " + index);
            root.transform.SetParent(transform, false);

            Transform body = CreatePart(root.transform, PrimitiveType.Cube, "Body", new Vector3(0f, 0.34f, 0f), new Vector3(0.38f, 0.22f, 0.62f), new Color(0.28f, 0.62f, 0.4f));
            Transform head = CreatePart(root.transform, PrimitiveType.Sphere, "Head", new Vector3(0f, 0.44f, 0.42f), new Vector3(0.2f, 0.18f, 0.22f), new Color(0.34f, 0.78f, 0.5f));
            Transform tail = CreatePart(root.transform, PrimitiveType.Cylinder, "Tail", new Vector3(0f, 0.28f, -0.48f), new Vector3(0.06f, 0.18f, 0.06f), new Color(0.18f, 0.42f, 0.28f));
            Transform legFrontLeft = CreatePart(root.transform, PrimitiveType.Capsule, "LegFrontLeft", new Vector3(-0.14f, 0.14f, 0.18f), new Vector3(0.08f, 0.2f, 0.08f), new Color(0.16f, 0.28f, 0.2f));
            Transform legFrontRight = CreatePart(root.transform, PrimitiveType.Capsule, "LegFrontRight", new Vector3(0.14f, 0.14f, 0.18f), new Vector3(0.08f, 0.2f, 0.08f), new Color(0.16f, 0.28f, 0.2f));
            Transform legBackLeft = CreatePart(root.transform, PrimitiveType.Capsule, "LegBackLeft", new Vector3(-0.14f, 0.14f, -0.18f), new Vector3(0.08f, 0.2f, 0.08f), new Color(0.16f, 0.28f, 0.2f));
            Transform legBackRight = CreatePart(root.transform, PrimitiveType.Capsule, "LegBackRight", new Vector3(0.14f, 0.14f, -0.18f), new Vector3(0.08f, 0.2f, 0.08f), new Color(0.16f, 0.28f, 0.2f));

            return new CreatureRig
            {
                Root = root.transform,
                Body = body,
                Head = head,
                Tail = tail,
                LegFrontLeft = legFrontLeft,
                LegFrontRight = legFrontRight,
                LegBackLeft = legBackLeft,
                LegBackRight = legBackRight
            };
        }

        private void ApplyRigPose(CreatureRig rig, Vector3 position, Vector3 velocity, float phase)
        {
            float bob = Mathf.Sin(phase * 2f) * 0.035f;
            float frontLeg = Mathf.Sin(phase) * 28f;
            float rearLeg = Mathf.Sin(phase + Mathf.PI) * 28f;

            rig.Root.localPosition = position;
            Vector3 flatVelocity = velocity;
            flatVelocity.y = 0f;
            if (flatVelocity.sqrMagnitude > 0.0001f)
                rig.Root.rotation = Quaternion.LookRotation(flatVelocity.normalized, Vector3.up);

            rig.Body.localPosition = new Vector3(0f, 0.34f + bob, 0f);
            rig.Body.localRotation = Quaternion.Euler(Mathf.Sin(phase * 0.5f) * 6f, 0f, Mathf.Sin(phase) * 2f);
            rig.Head.localRotation = Quaternion.Euler(Mathf.Sin(phase * 0.4f) * 4f, Mathf.Sin(phase * 0.6f) * 8f, 0f);
            rig.Tail.localRotation = Quaternion.Euler(90f, Mathf.Sin(phase * 1.4f) * 18f, 0f);
            rig.LegFrontLeft.localRotation = Quaternion.Euler(frontLeg, 0f, 0f);
            rig.LegFrontRight.localRotation = Quaternion.Euler(-frontLeg, 0f, 0f);
            rig.LegBackLeft.localRotation = Quaternion.Euler(rearLeg, 0f, 0f);
            rig.LegBackRight.localRotation = Quaternion.Euler(-rearLeg, 0f, 0f);
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

        private void ClearRigs()
        {
            if (rigs == null)
                return;

            for (int i = 0; i < rigs.Length; i++)
            {
                if (rigs[i].Root != null)
                    DestroyVisual(rigs[i].Root.gameObject);
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
