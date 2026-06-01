namespace LearningArchitect.UI
{
    public readonly struct MetricsSample
    {
        public MetricsSample(float fps, float frameTimeMs)
        {
            Fps = fps;
            FrameTimeMs = frameTimeMs;
        }

        public float Fps { get; }
        public float FrameTimeMs { get; }
    }
}
