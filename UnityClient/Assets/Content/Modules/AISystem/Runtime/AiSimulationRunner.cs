using System;

namespace LearningArchitect.Modules.AI
{
    public sealed class AiSimulationRunner
    {
        private AiWorld world;
        private IAiDecisionModel model;

        public AiWorld World => world ?? throw new InvalidOperationException($"{nameof(AiSimulationRunner)} is not initialized.");

        public void Initialize(AiWorld simulationWorld, IAiDecisionModel decisionModel)
        {
            world = simulationWorld ?? throw new ArgumentNullException(nameof(simulationWorld));
            model = decisionModel ?? throw new ArgumentNullException(nameof(decisionModel));
        }

        public void Tick(float deltaTime)
        {
            if (world == null || model == null)
                throw new InvalidOperationException($"{nameof(AiSimulationRunner)} is not initialized.");

            model.Tick(world, deltaTime);
        }
    }
}
