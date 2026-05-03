using System;

namespace LearningArchitect.Core
{
    public readonly struct ShowcaseMetricsSnapshot : IEquatable<ShowcaseMetricsSnapshot>
    {
        public static readonly ShowcaseMetricsSnapshot Empty = new(-1, -1, -1, float.NaN);

        public ShowcaseMetricsSnapshot(int simulationCount, int visibleCount, int operationsPerFrame, float simulationTimeMs)
        {
            SimulationCount = simulationCount;
            VisibleCount = visibleCount;
            OperationsPerFrame = operationsPerFrame;
            SimulationTimeMs = simulationTimeMs;
        }

        public int SimulationCount { get; }
        public int VisibleCount { get; }
        public int OperationsPerFrame { get; }
        public float SimulationTimeMs { get; }

        public bool HasSimulationCount => SimulationCount >= 0;
        public bool HasVisibleCount => VisibleCount >= 0;
        public bool HasOperationsPerFrame => OperationsPerFrame >= 0;
        public bool HasSimulationTimeMs => !float.IsNaN(SimulationTimeMs) && SimulationTimeMs >= 0f;

        public ShowcaseMetricsSnapshot WithSimulationCount(int simulationCount)
        {
            return new ShowcaseMetricsSnapshot(simulationCount, VisibleCount, OperationsPerFrame, SimulationTimeMs);
        }

        public ShowcaseMetricsSnapshot WithVisibleCount(int visibleCount)
        {
            return new ShowcaseMetricsSnapshot(SimulationCount, visibleCount, OperationsPerFrame, SimulationTimeMs);
        }

        public bool Equals(ShowcaseMetricsSnapshot other)
        {
            return SimulationCount == other.SimulationCount &&
                   VisibleCount == other.VisibleCount &&
                   OperationsPerFrame == other.OperationsPerFrame &&
                   SimulationTimeMs.Equals(other.SimulationTimeMs);
        }

        public override bool Equals(object obj)
        {
            return obj is ShowcaseMetricsSnapshot other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SimulationCount, VisibleCount, OperationsPerFrame, SimulationTimeMs);
        }

        public static bool operator ==(ShowcaseMetricsSnapshot left, ShowcaseMetricsSnapshot right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ShowcaseMetricsSnapshot left, ShowcaseMetricsSnapshot right)
        {
            return !left.Equals(right);
        }
    }
}
