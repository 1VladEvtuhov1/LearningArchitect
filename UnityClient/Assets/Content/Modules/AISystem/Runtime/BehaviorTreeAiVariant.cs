using System;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.AI
{
    public sealed class BehaviorTreeAiVariant : MonoBehaviour, IShowcaseStressTarget, IShowcaseMetricsSource
    {
        private const int PatrolTask = 2;
        private const uint SeedSalt = 0xB7A4C209u;

        [SerializeField] private int count = 240;
        [SerializeField] private int visualLimit = 360;
        [SerializeField] private float radius = 8f;
        [SerializeField] private float moveSpeed = 1.9f;
        [SerializeField] private float recoverDistance = 1.3f;
        [SerializeField] private float recoverRate = 0.55f;
        [SerializeField] private float energyDrain = 0.22f;
        [SerializeField] private float inspectDuration = 1.2f;
        [SerializeField] private float inspectChance = 0.18f;
        [SerializeField] private float targetReachDistance = 0.4f;
        [SerializeField] [Range(0.1f, 2f)] private float patrolRadiusScale = 0.95f;
        [SerializeField] [Range(0.05f, 1f)] private float inspectRadiusScale = 0.28f;
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] private Vector3 visualScale = new(0.08f, 0.14f, 0.08f);

        private readonly AiSimulationHost simulationHost = new();

        public int ActiveCount => simulationHost.ActiveCount;

        private void Awake()
        {
            IAiDecisionModel model = new BehaviorTreeDecisionModel(
                radius,
                moveSpeed,
                recoverDistance,
                recoverRate,
                energyDrain,
                inspectDuration,
                inspectChance,
                targetReachDistance,
                patrolRadiusScale,
                inspectRadiusScale);

            int spawnCount = ShowcaseStressSpawn.Clamp(count, visualLimit);
            count = spawnCount;
            simulationHost.Initialize(
                transform,
                spawnCount,
                spawnCount,
                visualLimit,
                visualPrefab,
                visualScale,
                "BT Agent ",
                true,
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
            for (int i = 0; i < world.Count; i++)
            {
                uint randomState = AiDeterministicRandom.CreateState(i, SeedSalt);
                world.Positions[i] = AiDeterministicRandom.NextPointOnDisc(ref randomState, radius);
                world.HomePositions[i] = world.Positions[i];
                world.Targets[i] = world.HomePositions[i] + AiDeterministicRandom.NextPointOnDisc(ref randomState, radius * patrolRadiusScale);
                world.Energy[i] = 0.35f + (AiDeterministicRandom.NextFloat01(ref randomState) * 0.65f);
                world.Timers[i] = 0f;
                world.Task[i] = PatrolTask;
                world.RandomStates[i] = randomState;
            }
        }
    }
}
