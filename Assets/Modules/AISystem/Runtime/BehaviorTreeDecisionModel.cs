using System;
using UnityEngine;

namespace LearningArchitect.Modules.AI
{
    public sealed class BehaviorTreeDecisionModel : IAiDecisionModel
    {
        private const int RecoverTask = 0;
        private const int InspectTask = 1;
        private const int PatrolTask = 2;

        private readonly float radius;
        private readonly float moveSpeed;
        private readonly float recoverDistance;
        private readonly float recoverRate;
        private readonly float energyDrain;
        private readonly float inspectDuration;
        private readonly float inspectChance;
        private readonly float targetReachDistance;
        private readonly float patrolRadiusScale;
        private readonly float inspectRadiusScale;

        public BehaviorTreeDecisionModel(
            float radius,
            float moveSpeed,
            float recoverDistance,
            float recoverRate,
            float energyDrain,
            float inspectDuration,
            float inspectChance,
            float targetReachDistance,
            float patrolRadiusScale,
            float inspectRadiusScale)
        {
            if (radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(radius));
            if (moveSpeed <= 0f)
                throw new ArgumentOutOfRangeException(nameof(moveSpeed));
            if (recoverDistance <= 0f)
                throw new ArgumentOutOfRangeException(nameof(recoverDistance));
            if (recoverRate < 0f)
                throw new ArgumentOutOfRangeException(nameof(recoverRate));
            if (energyDrain < 0f)
                throw new ArgumentOutOfRangeException(nameof(energyDrain));
            if (inspectDuration < 0f)
                throw new ArgumentOutOfRangeException(nameof(inspectDuration));
            if (inspectChance < 0f)
                throw new ArgumentOutOfRangeException(nameof(inspectChance));
            if (targetReachDistance <= 0f)
                throw new ArgumentOutOfRangeException(nameof(targetReachDistance));
            if (patrolRadiusScale <= 0f)
                throw new ArgumentOutOfRangeException(nameof(patrolRadiusScale));
            if (inspectRadiusScale <= 0f)
                throw new ArgumentOutOfRangeException(nameof(inspectRadiusScale));

            this.radius = radius;
            this.moveSpeed = moveSpeed;
            this.recoverDistance = recoverDistance;
            this.recoverRate = recoverRate;
            this.energyDrain = energyDrain;
            this.inspectDuration = inspectDuration;
            this.inspectChance = inspectChance;
            this.targetReachDistance = targetReachDistance;
            this.patrolRadiusScale = patrolRadiusScale;
            this.inspectRadiusScale = inspectRadiusScale;
        }

        public void Tick(AiWorld world, float deltaTime)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            float recoverDistanceSquared = recoverDistance * recoverDistance;
            float targetReachDistanceSquared = targetReachDistance * targetReachDistance;

            for (int i = 0; i < world.Count; i++)
            {
                if (world.Timers[i] > 0f)
                    world.Timers[i] -= deltaTime;

                int task = SelectTask(world, i, recoverDistanceSquared);
                world.Task[i] = task;

                switch (task)
                {
                    case RecoverTask:
                        world.Targets[i] = world.HomePositions[i];
                        MoveTowards(world, i, deltaTime, targetReachDistanceSquared);
                        if ((world.Positions[i] - world.HomePositions[i]).sqrMagnitude <= recoverDistanceSquared)
                            world.Energy[i] = Mathf.Min(1f, world.Energy[i] + recoverRate * deltaTime);
                        break;

                    case InspectTask:
                        MoveTowards(world, i, deltaTime, targetReachDistanceSquared);
                        world.Energy[i] = Mathf.Max(0f, world.Energy[i] - energyDrain * deltaTime * 0.65f);
                        break;

                    case PatrolTask:
                        if ((world.Targets[i] - world.Positions[i]).sqrMagnitude <= targetReachDistanceSquared)
                        {
                            uint patrolRandomState = world.RandomStates[i];
                            world.Targets[i] = SelectTargetAroundHome(world, i, radius * patrolRadiusScale, ref patrolRandomState);
                            world.RandomStates[i] = patrolRandomState;
                        }

                        MoveTowards(world, i, deltaTime, targetReachDistanceSquared);
                        world.Energy[i] = Mathf.Max(0f, world.Energy[i] - energyDrain * deltaTime);

                        if (world.Timers[i] <= 0f)
                        {
                            uint inspectRandomState = world.RandomStates[i];
                            if (AiDeterministicRandom.NextFloat01(ref inspectRandomState) <= inspectChance * deltaTime)
                            {
                                world.Timers[i] = inspectDuration;
                                world.Targets[i] = SelectTargetAroundHome(world, i, radius * inspectRadiusScale, ref inspectRandomState);
                            }

                            world.RandomStates[i] = inspectRandomState;
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(world.Task), $"Unknown behavior task {task} at agent {i}.");
                }
            }
        }

        private int SelectTask(AiWorld world, int index, float recoverDistanceSquared)
        {
            if (world.Energy[index] <= 0.25f)
                return RecoverTask;

            if (world.Timers[index] > 0f)
                return InspectTask;

            Vector3 offsetFromHome = world.Positions[index] - world.HomePositions[index];
            if (offsetFromHome.sqrMagnitude > recoverDistanceSquared * 6f && world.Energy[index] <= 0.4f)
                return RecoverTask;

            return PatrolTask;
        }

        private static Vector3 SelectTargetAroundHome(AiWorld world, int index, float targetRadius, ref uint randomState)
        {
            return world.HomePositions[index] + AiDeterministicRandom.NextPointOnDisc(ref randomState, targetRadius);
        }

        private void MoveTowards(AiWorld world, int index, float deltaTime, float targetReachDistanceSquared)
        {
            Vector3 direction = world.Targets[index] - world.Positions[index];
            if (direction.sqrMagnitude <= targetReachDistanceSquared)
                return;

            world.Positions[index] += direction.normalized * moveSpeed * deltaTime;
        }
    }
}
