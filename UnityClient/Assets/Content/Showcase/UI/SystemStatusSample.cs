namespace LearningArchitect.UI
{
    public readonly struct SystemStatusSample
    {
        public SystemStatusSample(float cpuPercent, float gpuPercent, float memoryGb)
        {
            CpuPercent = cpuPercent;
            GpuPercent = gpuPercent;
            MemoryGb = memoryGb;
        }

        public float CpuPercent { get; }
        public float GpuPercent { get; }
        public float MemoryGb { get; }

        public bool HasCpuPercent => CpuPercent >= 0f;
        public bool HasGpuPercent => GpuPercent >= 0f;
    }
}
