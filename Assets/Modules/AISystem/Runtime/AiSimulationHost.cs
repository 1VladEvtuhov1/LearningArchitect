using System;
using System.Diagnostics;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.AI
{
    public sealed class AiSimulationHost : IDisposable, IShowcaseMetricsSource
    {
        private readonly AiSimulationRunner runner = new();
        private readonly AiViewPresenter presenter = new();

        private Transform parent;
        private IAiDecisionModel model;
        private Action<AiWorld> worldInitializer;
        private PrimitiveType primitiveType;
        private Vector3 visualScale;
        private string visualNamePrefix;
        private bool orientToTarget;
        private int visibleCount;
        private int visualLimit;
        private int activeCount;
        private bool initialized;
        private float moduleCpuMs;

        public int ActiveCount
        {
            get
            {
                EnsureInitialized();
                return presenter.VisibleCount;
            }
        }

        public AiWorld World => runner.World;

        public void Initialize(
            Transform simulationParent,
            int count,
            int requestedVisibleCount,
            int maxVisuals,
            PrimitiveType visualPrimitive,
            Vector3 scale,
            string namePrefix,
            bool shouldOrientToTarget,
            IAiDecisionModel decisionModel,
            Action<AiWorld> initializeWorld)
        {
            if (initialized)
                throw new InvalidOperationException($"{nameof(AiSimulationHost)} is already initialized.");

            parent = simulationParent != null ? simulationParent : throw new ArgumentNullException(nameof(simulationParent));
            model = decisionModel ?? throw new ArgumentNullException(nameof(decisionModel));
            worldInitializer = initializeWorld ?? throw new ArgumentNullException(nameof(initializeWorld));
            primitiveType = visualPrimitive;
            visualScale = scale;
            visualNamePrefix = namePrefix;
            orientToTarget = shouldOrientToTarget;
            visibleCount = requestedVisibleCount;
            visualLimit = maxVisuals;

            presenter.Configure(parent, primitiveType, visualScale, visualNamePrefix, orientToTarget, visibleCount, visualLimit);
            Rebuild(count);
            initialized = true;
        }

        public void Tick(float deltaTime)
        {
            EnsureInitialized();
            long startedAt = Stopwatch.GetTimestamp();
            runner.Tick(deltaTime);
            moduleCpuMs = (float)((Stopwatch.GetTimestamp() - startedAt) * 1000d / Stopwatch.Frequency);
            presenter.Sync(runner.World);
        }

        public void SetStressLevel(int count)
        {
            EnsureInitialized();
            Rebuild(count);
        }

        public void SetVisibleBudget(int requestedVisibleCount, int maxVisuals)
        {
            visibleCount = requestedVisibleCount >= 0 ? requestedVisibleCount : throw new ArgumentOutOfRangeException(nameof(requestedVisibleCount));
            visualLimit = maxVisuals >= 0 ? maxVisuals : throw new ArgumentOutOfRangeException(nameof(maxVisuals));

            presenter.Configure(parent, primitiveType, visualScale, visualNamePrefix, orientToTarget, visibleCount, visualLimit);
        }

        public void Dispose()
        {
            presenter.Dispose();
            initialized = false;
            activeCount = 0;
            moduleCpuMs = 0f;
        }

        public ShowcaseMetricsSnapshot GetMetricsSnapshot()
        {
            EnsureInitialized();
            return new ShowcaseMetricsSnapshot(activeCount, presenter.VisibleCount, moduleCpuMs);
        }

        private void Rebuild(int count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count));

            AiWorld world = new(count);
            worldInitializer(world);
            runner.Initialize(world, model);
            presenter.Rebuild(world);
            activeCount = count;
        }

        private void EnsureInitialized()
        {
            if (!initialized)
                throw new InvalidOperationException($"{nameof(AiSimulationHost)} is not initialized.");
        }
    }
}
