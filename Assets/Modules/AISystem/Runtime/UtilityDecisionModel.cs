using System;
using UnityEngine;

namespace LearningArchitect.Modules.AI
{
    public sealed class UtilityDecisionModel : IAiDecisionModel
    {
        private readonly float radius;
        private readonly float moveSpeed;
        private readonly float retargetInterval;
        private readonly float centerBiasDistance;

        public UtilityDecisionModel(float radius, float moveSpeed, float retargetInterval, float centerBiasDistance)
        {
            if (radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(radius));
            if (moveSpeed <= 0f)
                throw new ArgumentOutOfRangeException(nameof(moveSpeed));
            if (retargetInterval <= 0f)
                throw new ArgumentOutOfRangeException(nameof(retargetInterval));
            if (centerBiasDistance < 0f)
                throw new ArgumentOutOfRangeException(nameof(centerBiasDistance));

            this.radius = radius;
            this.moveSpeed = moveSpeed;
            this.retargetInterval = retargetInterval;
            this.centerBiasDistance = centerBiasDistance;
        }

        public void Tick(AiWorld world, float deltaTime)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            float centerBiasDistanceSquared = centerBiasDistance * centerBiasDistance;

            for (int i = 0; i < world.Count; i++)
            {
                world.Timers[i] -= deltaTime;
                if (world.Timers[i] <= 0f)
                {
                    uint randomState = world.RandomStates[i];
                    world.Timers[i] = retargetInterval;
                    world.Targets[i] = ChooseTarget(world, i, centerBiasDistanceSquared, ref randomState);
                    world.RandomStates[i] = randomState;
                }

                Vector3 direction = world.Targets[i] - world.Positions[i];
                if (direction.sqrMagnitude > 0.001f)
                    world.Positions[i] += direction.normalized * moveSpeed * deltaTime;
            }
        }

        private Vector3 ChooseTarget(AiWorld world, int index, float centerBiasDistanceSquared, ref uint randomState)
        {
            Vector3 home = world.HomePositions[index];
            Vector3 offsetFromHome = world.Positions[index] - home;
            if (offsetFromHome.sqrMagnitude > centerBiasDistanceSquared)
                return home;

            return home + AiDeterministicRandom.NextPointOnDisc(ref randomState, radius);
        }
    }
}
