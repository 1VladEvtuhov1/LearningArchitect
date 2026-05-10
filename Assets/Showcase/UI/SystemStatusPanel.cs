using LearningArchitect.Core;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
#if !UNITY_WEBGL || UNITY_EDITOR
using System.Diagnostics;
#endif

namespace LearningArchitect.UI
{
    public sealed class SystemStatusPanel : MonoBehaviour
    {
        [SerializeField] private float refreshInterval = 1f;
        [SerializeField] private float memoryBarMaxGb = 8f;
        [SerializeField] private int smoothingSamples = 2;
        [SerializeField] private Color healthyDotColor = default;
        [SerializeField] private Color warningDotColor = default;
        [SerializeField] private Color criticalDotColor = default;

        private TextMeshProUGUI[] metricLabels = new TextMeshProUGUI[3];
        private RectTransform[] metricBars = new RectTransform[3];
        private RectTransform[] metricFills = new RectTransform[3];
        private Image statusDot;
        private float nextRefreshTime;
#if !UNITY_WEBGL || UNITY_EDITOR
        private Process currentProcess;
        private DateTime lastCpuSampleTimeUtc;
        private TimeSpan lastCpuTotalTime;
#endif
        private float cpuPercent;
        private float gpuPercent = -1f;
        private float memoryGb;
        private float displayedCpuPercent;
        private float displayedGpuPercent = -1f;
        private float displayedMemoryGb;
        private float[] cpuSamples;
        private float[] gpuSamples;
        private float[] memorySamples;
        private int sampleIndex;
        private int sampleCount;

        private void Awake()
        {
            if (healthyDotColor == default)
                healthyDotColor = ShowcasePalette.Success;

            if (warningDotColor == default)
                warningDotColor = ShowcasePalette.Warning;

            if (criticalDotColor == default)
                criticalDotColor = ShowcasePalette.Error;

            smoothingSamples = Mathf.Max(1, smoothingSamples);
            cpuSamples = new float[smoothingSamples];
            gpuSamples = new float[smoothingSamples];
            memorySamples = new float[smoothingSamples];
            for (int i = 0; i < smoothingSamples; i++)
                gpuSamples[i] = -1f;

#if !UNITY_WEBGL || UNITY_EDITOR
            currentProcess = Process.GetCurrentProcess();
            lastCpuSampleTimeUtc = DateTime.UtcNow;
            lastCpuTotalTime = currentProcess.TotalProcessorTime;
#else
            cpuPercent = -1f;
            displayedCpuPercent = -1f;
#endif
            ResolveUi();
        }

        private void OnDestroy()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            currentProcess?.Dispose();
#endif
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefreshTime)
                return;

            nextRefreshTime = Time.unscaledTime + refreshInterval;
            SampleSystemCounters();
            RefreshStatus();
        }

        private void ResolveUi()
        {
            Canvas canvas = FindCanvasByChild(transform, "Container - Root");
            if (canvas == null)
                throw new InvalidOperationException($"{nameof(SystemStatusPanel)} requires a canvas containing Container - Root.");

            Transform card = FindDeep(canvas.transform, "Container - SystemStatus");
            if (card == null)
                throw new InvalidOperationException($"{nameof(SystemStatusPanel)} requires Container - SystemStatus.");

            string[] metricNames = { "Text - CpuMetric", "Text - GpuMetric", "Text - MemMetric" };
            string[] barNames = { "Image - CpuBar", "Image - GpuBar", "Image - MemBar" };
            for (int i = 0; i < 3; i++)
            {
                Transform metric = FindDeep(card, metricNames[i]);
                Transform bar = FindDeep(card, barNames[i]);
                if (metric == null || bar == null)
                    throw new InvalidOperationException($"{nameof(SystemStatusPanel)} requires {metricNames[i]} and {barNames[i]}.");

                metricLabels[i] = metric.GetComponent<TextMeshProUGUI>();
                metricBars[i] = bar.GetComponent<RectTransform>();
                metricFills[i] = bar.Find("Fill") as RectTransform;

                if (metricLabels[i] == null || metricBars[i] == null || metricFills[i] == null)
                    throw new InvalidOperationException($"Status metric slot {i} is not fully configured.");
            }

            Transform dot = FindDeep(card, "StatusDot");
            if (dot == null)
                throw new InvalidOperationException($"{nameof(SystemStatusPanel)} requires StatusDot.");

            statusDot = dot.GetComponent<Image>();
            if (statusDot == null)
                throw new InvalidOperationException("StatusDot requires Image.");

            RefreshStatus();
        }

        private void RefreshStatus()
        {
            float cpuNormalized = displayedCpuPercent < 0f ? 0f : Mathf.Clamp01(displayedCpuPercent / 100f);
            float gpuNormalized = displayedGpuPercent < 0f ? 0f : Mathf.Clamp01(displayedGpuPercent / 100f);
            float memNormalized = Mathf.Clamp01(displayedMemoryGb / Mathf.Max(0.1f, memoryBarMaxGb));

            SetMetric(0, "CPU", displayedCpuPercent < 0f ? ShowcaseLocalization.GetText("na") : Mathf.RoundToInt(displayedCpuPercent) + "%", cpuNormalized);
            SetMetric(1, "GPU", displayedGpuPercent < 0f ? ShowcaseLocalization.GetText("na") : Mathf.RoundToInt(displayedGpuPercent) + "%", gpuNormalized);
            SetMetric(2, "MEM", displayedMemoryGb.ToString("0.0", CultureInfo.InvariantCulture) + " GB", memNormalized);

            float statusLoad = Mathf.Max(
                displayedCpuPercent < 0f ? 0f : displayedCpuPercent,
                displayedGpuPercent < 0f ? 0f : displayedGpuPercent);
            float statusThreshold = statusLoad > 0f ? statusLoad : memNormalized * 100f;

            if (statusThreshold >= 90f)
                statusDot.color = criticalDotColor;
            else if (statusThreshold >= 70f)
                statusDot.color = warningDotColor;
            else
                statusDot.color = healthyDotColor;
        }

        private void SampleSystemCounters()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (currentProcess == null)
                currentProcess = Process.GetCurrentProcess();

            currentProcess.Refresh();

            DateTime nowUtc = DateTime.UtcNow;
            TimeSpan currentCpuTime = currentProcess.TotalProcessorTime;
            double wallSeconds = (nowUtc - lastCpuSampleTimeUtc).TotalSeconds;
            double cpuSeconds = (currentCpuTime - lastCpuTotalTime).TotalSeconds;

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

            memoryGb = memoryBytes / (1024f * 1024f * 1024f);
            gpuPercent = SampleGpuPercent(currentProcess.Id);
            PushSample(cpuPercent, gpuPercent, memoryGb);
#else
            memoryGb = GetProfilerMemoryGb();
            PushSample(-1f, -1f, memoryGb);
#endif
        }

        private void PushSample(float cpu, float gpu, float memory)
        {
            if (cpuSamples == null || cpuSamples.Length == 0)
                return;

            cpuSamples[sampleIndex] = cpu;
            gpuSamples[sampleIndex] = gpu;
            memorySamples[sampleIndex] = memory;

            sampleIndex = (sampleIndex + 1) % cpuSamples.Length;
            sampleCount = Mathf.Min(sampleCount + 1, cpuSamples.Length);

            displayedCpuPercent = AverageNonNegative(cpuSamples, sampleCount);
            displayedMemoryGb = Average(memorySamples, sampleCount);
            displayedGpuPercent = AverageNonNegative(gpuSamples, sampleCount);
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

        private static float SampleGpuPercent(int processId)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            try
            {
                Type categoryType = Type.GetType("System.Diagnostics.PerformanceCounterCategory, System.Diagnostics.PerformanceCounter");
                Type counterType = Type.GetType("System.Diagnostics.PerformanceCounter, System.Diagnostics.PerformanceCounter");
                if (categoryType == null || counterType == null)
                    return -1f;

                object category = Activator.CreateInstance(categoryType, "GPU Engine");
                string[] instanceNames = categoryType.GetMethod("GetInstanceNames", Type.EmptyTypes).Invoke(category, null) as string[];
                if (instanceNames == null)
                    return -1f;

                float total = 0f;
                string pidToken = "pid_" + processId.ToString(CultureInfo.InvariantCulture);

                for (int i = 0; i < instanceNames.Length; i++)
                {
                    string instanceName = instanceNames[i];
                    if (!instanceName.Contains(pidToken) || !instanceName.Contains("engtype_3D"))
                        continue;

                    IDisposable counter = Activator.CreateInstance(counterType, "GPU Engine", "Utilization Percentage", instanceName, true) as IDisposable;
                    if (counter == null)
                        continue;

                    using (counter)
                    {
                        object rawValue = counterType.GetMethod("NextValue", Type.EmptyTypes).Invoke(counter, null);
                        if (rawValue is float sample)
                            total += sample;
                    }
                }

                return Mathf.Clamp(total, 0f, 100f);
            }
            catch
            {
                return -1f;
            }
#else
            return -1f;
#endif
        }

        private void SetMetric(int index, string title, string value, float normalized)
        {
            metricLabels[index].text = title + " " + value;

            RectTransform bar = metricBars[index];
            RectTransform fill = metricFills[index];
            float width = Mathf.Max(4f, (bar.rect.width - 2f) * Mathf.Clamp01(normalized));
            fill.anchorMin = new Vector2(0f, 0f);
            fill.anchorMax = new Vector2(0f, 1f);
            fill.pivot = new Vector2(0f, 0.5f);
            fill.anchoredPosition = new Vector2(1f, 0f);
            fill.sizeDelta = new Vector2(width, -2f);
        }

        private static Canvas FindCanvasByChild(Transform root, string childName)
        {
            Canvas[] canvases = root.GetComponentsInChildren<Canvas>(true);
            for (int i = 0; i < canvases.Length; i++)
            {
                if (FindDeep(canvases[i].transform, childName) != null)
                    return canvases[i];
            }

            return null;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null)
                return null;

            if (root.name == name)
                return root;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDeep(root.GetChild(i), name);
                if (match != null)
                    return match;
            }

            return null;
        }
    }
}
