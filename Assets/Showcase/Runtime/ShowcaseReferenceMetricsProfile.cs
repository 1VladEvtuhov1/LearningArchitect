namespace LearningArchitect.Core
{
    public sealed class ShowcaseReferenceMetricsProfile
    {
        public ShowcaseReferenceMetricsProfile(
            float fps,
            float frameTimeMs,
            int simulationCount,
            int visibleCount,
            int operationsPerFrame,
            float simulationCpuMs,
            float[] graphSamples)
        {
            Fps = fps;
            FrameTimeMs = frameTimeMs;
            SimulationCount = simulationCount;
            VisibleCount = visibleCount;
            OperationsPerFrame = operationsPerFrame;
            SimulationCpuMs = simulationCpuMs;
            GraphSamples = graphSamples;
        }

        public float Fps { get; }
        public float FrameTimeMs { get; }
        public int SimulationCount { get; }
        public int VisibleCount { get; }
        public int OperationsPerFrame { get; }
        public float SimulationCpuMs { get; }
        public float[] GraphSamples { get; }
    }
}
