using LearningArchitect.Modules.AI;
using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Tests.AI
{
    [Category("LearningArchitect.AI.Edit")]
    public sealed class AiSimulationTests
    {
        [Test]
        public void SimulationRunnerDelegatesToDecisionModel()
        {
            AiWorld world = new(1);
            CountingDecisionModel model = new();
            AiSimulationRunner runner = new();

            runner.Initialize(world, model);
            runner.Tick(0.25f);

            Assert.AreEqual(1, model.TickCount);
            Assert.AreEqual(0.25f, model.LastDeltaTime);
        }

        [Test]
        public void FsmDecisionModelTransitionsIdleToMove()
        {
            AiWorld world = new(1);
            world.State[0] = 0;
            world.Timers[0] = 0f;
            world.RandomStates[0] = 1u;

            FsmDecisionModel model = new(8f, 1.8f, 0.6f, 0.35f);
            model.Tick(world, 0.1f);

            Assert.AreEqual(1, world.State[0]);
        }

        [Test]
        public void UtilityDecisionModelBiasesAgentsBackToHome()
        {
            AiWorld world = new(1);
            world.Positions[0] = new Vector3(5f, 0f, 0f);
            world.HomePositions[0] = new Vector3(2f, 0f, 0f);
            world.Timers[0] = 0f;
            world.RandomStates[0] = 2u;

            UtilityDecisionModel model = new(8f, 2f, 0.4f, 2.4f);
            model.Tick(world, 1f);

            Assert.AreEqual(world.HomePositions[0], world.Targets[0]);
            Assert.Less(world.Positions[0].sqrMagnitude, 25f);
        }

        [Test]
        public void BehaviorTreeDecisionModelRecoversLowEnergyAgents()
        {
            AiWorld world = new(1);
            world.Positions[0] = new Vector3(0.2f, 0f, 0.1f);
            world.HomePositions[0] = new Vector3(0.1f, 0f, 0.1f);
            world.Targets[0] = new Vector3(3f, 0f, 0f);
            world.Energy[0] = 0.1f;
            world.RandomStates[0] = 3u;

            BehaviorTreeDecisionModel model = new(8f, 1.9f, 1.3f, 0.55f, 0.22f, 1.2f, 0.18f, 0.4f, 0.95f, 0.28f);
            model.Tick(world, 1f);

            Assert.AreEqual(0, world.Task[0]);
            Assert.Greater(world.Energy[0], 0.1f);
            Assert.AreEqual(world.HomePositions[0], world.Targets[0]);
        }

        [Test]
        public void BehaviorTreeDecisionModelKeepsPatrolTargetsNearHome()
        {
            AiWorld world = new(1);
            world.Positions[0] = new Vector3(3f, 0f, 1f);
            world.HomePositions[0] = new Vector3(3f, 0f, 1f);
            world.Targets[0] = world.Positions[0];
            world.Energy[0] = 0.9f;
            world.Timers[0] = 0f;
            world.Task[0] = 2;
            world.RandomStates[0] = 7u;

            BehaviorTreeDecisionModel model = new(8f, 1.9f, 1.3f, 0.55f, 0.22f, 1.2f, 0.18f, 0.4f, 0.95f, 0.28f);
            model.Tick(world, 0.016f);

            float distanceFromHome = (world.Targets[0] - world.HomePositions[0]).magnitude;
            Assert.LessOrEqual(distanceFromHome, 8f * 0.95f + 0.001f);
        }

        [Test]
        public void ViewPresenterCapsVisibleObjects()
        {
            GameObject root = new("AI Presenter Root");

            try
            {
                AiWorld world = new(6);
                AiViewPresenter presenter = new();
                presenter.Configure(root.transform, PrimitiveType.Sphere, Vector3.one, "AI Agent ", false, 4, 2);
                presenter.Rebuild(world);

                Assert.AreEqual(2, root.transform.childCount);

                presenter.Dispose();
                Assert.AreEqual(0, root.transform.childCount);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private sealed class CountingDecisionModel : IAiDecisionModel
        {
            public int TickCount { get; private set; }
            public float LastDeltaTime { get; private set; }

            public void Tick(AiWorld world, float deltaTime)
            {
                TickCount++;
                LastDeltaTime = deltaTime;
            }
        }
    }
}
