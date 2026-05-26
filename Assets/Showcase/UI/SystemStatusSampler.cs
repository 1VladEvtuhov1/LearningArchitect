using System;
using UnityEngine;
using UnityEngine.Profiling;
#if !UNITY_WEBGL || UNITY_EDITOR
using System.Diagnostics;
#endif

namespace LearningArchitect.UI
{
    public sealed class SystemStatusSampler : IDisposable
    {
        private readonly float[] cpuSamples;
        private readonly float[] gpuSamples;
        private readonly float[] memorySamples;

        private int sampleIndex;
        private int sampleCount;

#if !UNITY_WEBGL || UNITY_EDITOR
        private Process currentProcess;
        private DateTime lastCpuSampleTimeUtc;
        private TimeSpan lastCpuTotalTime;
#endif

        public SystemStatusSampler(int smoothingSamples)
        {
            int capacity = Mathf.Max(1, smoothingSamples);
            cpuSamples = new float[capacity];
            gpuSamples = new float[capacity];
            memorySamples = new float[capacity];

            for (int i = 0; i < capacity; i++)
                gpuSamples[i] = -1f;

#if !UNITY_WEBGL || UNITY_EDITOR
            currentProcess = Process.GetCurrentProcess();
            lastCpuSampleTimeUtc = DateTime.UtcNow;
            lastCpuTotalTime = currentProcess.TotalProcessorTime;
#endif
        }

        public SystemStatusSample Sample()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (currentProcess == null)
                currentProcess = Process.GetCurrentProcess();

            currentProcess.Refresh();

            DateTime nowUtc = DateTime.UtcNow;
            TimeSpan currentCpuTime = currentProcess.TotalProcessorTime;
            double wallSeconds = (nowUtc - lastCpuSampleTimeUtc).TotalSeconds;
            double cpuSeconds = (currentCpuTime - lastCpuTotalTime).TotalSeconds;

            float cpuPercent = -1f;
            if (wallSeconds > 0.0001d)
            {
                double usage = cpuSeconds / (wallSeconds * Math.Max(1, Environment.ProcessorCount)) * 100d;
                cpuPercent = Mathf.Clamp((float)usage, 0f, 100f);
            }

            lastCpuSampleTimeUtc = nowUtc;
            lastCpuTotalTime = currentCpuTime;

            long processMemoryBytes = Math.Max(currentProcess.WorkingSet64, Math.Max(currentProcess.PrivateMemorySize64, currentProcess.PagedMemorySize64));
            long profilerMemoryBytes = Math.Max(Profiler.GetTotalReservedMemoryLong(), Profiler.GetTotalAllocatedMemoryLong());
            long memoryBytes = Math.Max(processMemoryBytes, profilerMemoryBytes);
            float memoryGb = memoryBytes / (1024f * 1024f * 1024f);

            float gpuPercent = SampleGpuPercent();
            return PushSample(cpuPercent, gpuPercent, memoryGb);
#else
            return PushSample(-1f, -1f, GetProfilerMemoryGb());
#endif
        }

        public void Dispose()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            currentProcess?.Dispose();
#endif
        }

        private SystemStatusSample PushSample(float cpuPercent, float gpuPercent, float memoryGb)
        {
            cpuSamples[sampleIndex] = cpuPercent;
            gpuSamples[sampleIndex] = gpuPercent;
            memorySamples[sampleIndex] = memoryGb;

            sampleIndex = (sampleIndex + 1) % cpuSamples.Length;
            sampleCount = Mathf.Min(sampleCount + 1, cpuSamples.Length);

            return new SystemStatusSample(
                AverageNonNegative(cpuSamples, sampleCount),
                AverageNonNegative(gpuSamples, sampleCount),
                Average(memorySamples, sampleCount));
        }

        private static float GetProfilerMemoryGb()
        {
            long memoryBytes = Math.Max(Profiler.GetTotalReservedMemoryLong(), Profiler.GetTotalAllocatedMemoryLong());
            return memoryBytes / (1024f * 1024f * 1024f);
        }

        private static float Average(float[] values, int count)
        {
            if (values == null || count <= 0)
                return 0f;

            float total = 0f;
            for (int i = 0; i < count; i++)
                total += values[i];

            return total / count;
        }

        private static float AverageNonNegative(float[] values, int count)
        {
            if (values == null || count <= 0)
                return -1f;

            float total = 0f;
            int validCount = 0;
            for (int i = 0; i < count; i++)
            {
                if (values[i] < 0f)
                    continue;

                total += values[i];
                validCount++;
            }

            return validCount == 0 ? -1f : total / validCount;
        }

        private static float SampleGpuPercent()
        {
            // System.Diagnostics.PerformanceCounter is not in Unity's .NET Standard 2.1 reference set, so
            // Task-Manager-style GPU % is not sampled here. CPU and memory still use Process (available).
            return -1f;
        }
    }
}
