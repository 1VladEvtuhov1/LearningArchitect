using System;

namespace LearningArchitect.Core
{
    public readonly struct ShowcaseMetricsSnapshot : IEquatable<ShowcaseMetricsSnapshot>
    {
        public static readonly ShowcaseMetricsSnapshot Empty = new(-1, -1, float.NaN);

        public ShowcaseMetricsSnapshot(int simulationCount, int visibleCount, float moduleCpuMs)
        {
            SimulationCount = simulationCount;
            VisibleCount = visibleCount;
            ModuleCpuMs = moduleCpuMs;
        }

        public int SimulationCount { get; }
        public int VisibleCount { get; }
        public float ModuleCpuMs { get; }

        public bool HasSimulationCount => SimulationCount >= 0;
        public bool HasVisibleCount => VisibleCount >= 0;
        public bool HasModuleCpuMs => !float.IsNaN(ModuleCpuMs) && ModuleCpuMs >= 0f;

        public ShowcaseMetricsSnapshot WithSimulationCount(int simulationCount)
        {
            return new ShowcaseMetricsSnapshot(simulationCount, VisibleCount, ModuleCpuMs);
        }

        public ShowcaseMetricsSnapshot WithVisibleCount(int visibleCount)
        {
            return new ShowcaseMetricsSnapshot(SimulationCount, visibleCount, ModuleCpuMs);
        }

        public bool Equals(ShowcaseMetricsSnapshot other)
        {
            return SimulationCount == other.SimulationCount &&
                   VisibleCount == other.VisibleCount &&
                   ModuleCpuMs.Equals(other.ModuleCpuMs);
        }

        public override bool Equals(object obj)
        {
            return obj is ShowcaseMetricsSnapshot other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SimulationCount, VisibleCount, ModuleCpuMs);
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
