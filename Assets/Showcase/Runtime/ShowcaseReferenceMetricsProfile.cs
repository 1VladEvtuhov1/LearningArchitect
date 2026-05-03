namespace LearningArchitect.Core
{
    public sealed class ShowcaseReferenceMetricsProfile
    {
        public ShowcaseReferenceMetricsProfile(
            float fps,
            float frameTimeMs,
            int simulationCount,
            int visibleCount,
            float moduleCpuMs,
            float[] graphSamples)
        {
            Fps = fps;
            FrameTimeMs = frameTimeMs;
            SimulationCount = simulationCount;
            VisibleCount = visibleCount;
            ModuleCpuMs = moduleCpuMs;
            GraphSamples = graphSamples;
        }

        public float Fps { get; }
        public float FrameTimeMs { get; }
        public int SimulationCount { get; }
        public int VisibleCount { get; }
        public float ModuleCpuMs { get; }
        public float[] GraphSamples { get; }
    }
}
