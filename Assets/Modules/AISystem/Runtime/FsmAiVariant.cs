using System;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.AI
{
    public sealed class FsmAiVariant : MonoBehaviour, IShowcaseStressTarget, IShowcaseMetricsSource
    {
        private const int MoveState = 1;
        private const uint SeedSalt = 0xF51A23D1u;

        [SerializeField] private int count = 5000;
        [SerializeField] private int visibleCount = 220;
        [SerializeField] private int[] visibleCountStressPresets = { 500, 2000, 5000 };
        [SerializeField] private int[] visibleCountPresets = { 140, 220, 320 };
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
            simulationHost.Initialize(
                transform,
                count,
                ResolveVisibleCount(count),
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
