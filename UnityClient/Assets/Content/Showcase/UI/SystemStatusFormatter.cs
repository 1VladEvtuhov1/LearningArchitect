using System.Globalization;

namespace LearningArchitect.UI
{
    public static class SystemStatusFormatter
    {
        public static string FormatCpu(SystemStatusSample sample)
        {
            return "CPU " + (sample.HasCpuPercent ? FormatPercent(sample.CpuPercent) : ShowcaseLocalization.GetText("na"));
        }

        public static string FormatGpu(SystemStatusSample sample)
        {
            return "GPU " + (sample.HasGpuPercent ? FormatPercent(sample.GpuPercent) : ShowcaseLocalization.GetText("na"));
        }

        public static string FormatMemory(SystemStatusSample sample)
        {
            return "MEM " + sample.MemoryGb.ToString("0.0", CultureInfo.InvariantCulture) + " GB";
        }

        private static string FormatPercent(float value)
        {
            return value.ToString("0", CultureInfo.InvariantCulture) + "%";
        }
    }
}
