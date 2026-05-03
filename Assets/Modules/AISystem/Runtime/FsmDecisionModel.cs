using System;
using UnityEngine;

namespace LearningArchitect.Modules.AI
{
    public sealed class FsmDecisionModel : IAiDecisionModel
    {
        private const int IdleState = 0;
        private const int MoveState = 1;

        private readonly float radius;
        private readonly float moveSpeed;
        private readonly float idleDuration;
        private readonly float targetReachDistance;

        public FsmDecisionModel(float radius, float moveSpeed, float idleDuration, float targetReachDistance)
        {
            if (radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(radius));
            if (moveSpeed <= 0f)
                throw new ArgumentOutOfRangeException(nameof(moveSpeed));
            if (idleDuration < 0f)
                throw new ArgumentOutOfRangeException(nameof(idleDuration));
            if (targetReachDistance <= 0f)
                throw new ArgumentOutOfRangeException(nameof(targetReachDistance));

            this.radius = radius;
            this.moveSpeed = moveSpeed;
            this.idleDuration = idleDuration;
            this.targetReachDistance = targetReachDistance;
        }

        public void Tick(AiWorld world, float deltaTime)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            float reachDistanceSquared = targetReachDistance * targetReachDistance;

            for (int i = 0; i < world.Count; i++)
            {
                switch (world.State[i])
                {
                    case IdleState:
                        world.Timers[i] -= deltaTime;
                        if (world.Timers[i] <= 0f)
                        {
                            uint randomState = world.RandomStates[i];
                            world.State[i] = MoveState;
                            world.Targets[i] = AiDeterministicRandom.NextPointOnDisc(ref randomState, radius);
                            world.RandomStates[i] = randomState;
                        }
                        break;

                    case MoveState:
                        Vector3 direction = world.Targets[i] - world.Positions[i];
                        if (direction.sqrMagnitude <= reachDistanceSquared)
                        {
                            world.State[i] = IdleState;
                            world.Timers[i] = idleDuration;
                        }
                        else
                        {
                            world.Positions[i] += direction.normalized * moveSpeed * deltaTime;
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(world.State), $"Unknown FSM state {world.State[i]} at agent {i}.");
                }
            }
        }
    }
}
