using System;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.AI
{
    public sealed class FsmAiVariant : MonoBehaviour, IShowcaseStressTarget, IShowcaseMetricsSource
    {
        private const int MoveState = 1;
        private const uint SeedSalt = 0xF51A23D1u;

        [SerializeField] private int count = 240;
        [SerializeField] private int visualLimit = 360;
        [SerializeField] private float radius = 8f;
        [SerializeField] private float moveSpeed = 1.8f;
        [SerializeField] private float idleDuration = 0.6f;
        [SerializeField] private float targetReachDistance = 0.35f;
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] private Vector3 visualScale = new(0.12f, 0.22f, 0.12f);

        private readonly AiSimulationHost simulationHost = new();

        public int ActiveCount => simulationHost.ActiveCount;

        private void Awake()
        {
            IAiDecisionModel model = new FsmDecisionModel(radius, moveSpeed, idleDuration, targetReachDistance);
            int spawnCount = ShowcaseStressSpawn.Clamp(count, visualLimit);
            count = spawnCount;
            simulationHost.Initialize(
                transform,
                spawnCount,
                spawnCount,
                visualLimit,
                visualPrefab,
                visualScale,
                "FSM Agent ",
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
                world.Targets[i] = AiDeterministicRandom.NextPointOnDisc(ref randomState, radius);
                world.Timers[i] = 0f;
                world.State[i] = MoveState;
                world.RandomStates[i] = randomState;
            }
        }
    }
}
