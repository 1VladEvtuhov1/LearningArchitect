using System;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.AI
{
    public sealed class UtilityAiVariant : MonoBehaviour, IShowcaseStressTarget, IShowcaseMetricsSource
    {
        private const uint SeedSalt = 0x71C4A91Bu;

        [SerializeField] private int count = 5000;
        [SerializeField] private int visibleCount = 220;
        [SerializeField] private int[] visibleCountStressPresets = { 500, 2000, 5000 };
        [SerializeField] private int[] visibleCountPresets = { 140, 220, 320 };
        [SerializeField] private int visualLimit = 360;
        [SerializeField] private float radius = 8f;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float retargetInterval = 0.4f;
        [SerializeField] private float centerBiasDistance = 2.4f;

        private readonly AiSimulationHost simulationHost = new();

        public int ActiveCount => simulationHost.ActiveCount;

        private void Awake()
        {
            IAiDecisionModel model = new UtilityDecisionModel(radius, moveSpeed, retargetInterval, centerBiasDistance);
            simulationHost.Initialize(
                transform,
                count,
                ResolveVisibleCount(count),
                visualLimit,
                PrimitiveType.Sphere,
                Vector3.one * 0.14f,
                "Utility Agent ",
                false,
                model,
                InitializeWorld);
        }

        private void Update()
        {
            simulationHost.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            simulationHost.Dispose();
        }

        public void SetStressLevel(int value)
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (count == value)
                return;

            count = value;
            simulationHost.SetVisibleBudget(ResolveVisibleCount(count), visualLimit);
            simulationHost.SetStressLevel(count);
        }

        public ShowcaseMetricsSnapshot GetMetricsSnapshot()
        {
            return simulationHost.GetMetricsSnapshot();
        }

        private int ResolveVisibleCount(int targetCount)
        {
            if (visibleCountStressPresets == null || visibleCountPresets == null)
                return visibleCount;

            int pairCount = Mathf.Min(visibleCountStressPresets.Length, visibleCountPresets.Length);
            if (pairCount == 0)
                return visibleCount;

            for (int i = 0; i < pairCount; i++)
            {
                if (visibleCountStressPresets[i] == targetCount)
                    return Mathf.Max(1, visibleCountPresets[i]);
            }

            int bestIndex = 0;
            int smallestDistance = Mathf.Abs(visibleCountStressPresets[0] - targetCount);
            for (int i = 1; i < pairCount; i++)
            {
                int distance = Mathf.Abs(visibleCountStressPresets[i] - targetCount);
                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    bestIndex = i;
                }
            }

            return Mathf.Max(1, visibleCountPresets[bestIndex]);
        }

        private void InitializeWorld(AiWorld world)
        {
            float centerBiasDistanceSquared = centerBiasDistance * centerBiasDistance;

            for (int i = 0; i < world.Count; i++)
            {
                uint randomState = AiDeterministicRandom.CreateState(i, SeedSalt);
                world.Positions[i] = AiDeterministicRandom.NextPointOnDisc(ref randomState, radius);
                world.HomePositions[i] = world.Positions[i];
                world.Targets[i] = ChooseInitialTarget(world.Positions[i], world.HomePositions[i], centerBiasDistanceSquared, ref randomState);
                world.Timers[i] = AiDeterministicRandom.NextFloat01(ref randomState) * retargetInterval;
                world.RandomStates[i] = randomState;
            }
        }

        private Vector3 ChooseInitialTarget(Vector3 position, Vector3 homePosition, float centerBiasDistanceSquared, ref uint randomState)
        {
            if ((position - homePosition).sqrMagnitude > centerBiasDistanceSquared)
                return homePosition;

            return homePosition + AiDeterministicRandom.NextPointOnDisc(ref randomState, radius);
        }
    }
}
