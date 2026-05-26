using System;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.AI
{
    public sealed class UtilityAiVariant : MonoBehaviour, IShowcaseStressTarget, IShowcaseMetricsSource
    {
        private const uint SeedSalt = 0x71C4A91Bu;

        [SerializeField] private int count = 240;
        [SerializeField] private int visualLimit = 360;
        [SerializeField] private float radius = 8f;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float retargetInterval = 0.4f;
        [SerializeField] private float centerBiasDistance = 2.4f;
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] private Vector3 visualScale = Vector3.one * 0.14f;

        private readonly AiSimulationHost simulationHost = new();

        public int ActiveCount => simulationHost.ActiveCount;

        private void Awake()
        {
            IAiDecisionModel model = new UtilityDecisionModel(radius, moveSpeed, retargetInterval, centerBiasDistance);
            int spawnCount = ShowcaseStressSpawn.Clamp(count, visualLimit);
            count = spawnCount;
            simulationHost.Initialize(
                transform,
                spawnCount,
                spawnCount,
                visualLimit,
                visualPrefab,
                visualScale,
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

            int spawnCount = ShowcaseStressSpawn.Clamp(value, visualLimit);
            if (count == spawnCount)
                return;

            count = spawnCount;
            simulationHost.SetVisibleBudget(spawnCount, visualLimit);
            simulationHost.SetStressLevel(spawnCount);
        }

        public ShowcaseMetricsSnapshot GetMetricsSnapshot()
        {
            return simulationHost.GetMetricsSnapshot();
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
