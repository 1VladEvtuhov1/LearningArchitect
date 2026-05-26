using System.Globalization;

namespace LearningArchitect.UI
{
    public static class MetricsOverlayFormatter
    {
        public static string GetHeaderText()
        {
            return ShowcaseLocalization.GetText("performance");
        }

        public static string GetFpsLabel()
        {
            return ShowcaseLocalization.GetText("fps");
        }

        public static string GetFrameTimeLabel()
        {
            return ShowcaseLocalization.GetText("frame_time");
        }

        public static string GetActiveItemsLabel()
        {
            return ShowcaseLocalization.GetText("active_items");
        }

        public static string FormatCount(int value)
        {
            return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
        }

        public static string FormatFps(float fps)
        {
            return fps.ToString("0", CultureInfo.InvariantCulture);
        }

        public static string FormatFrameTime(float frameTimeMs)
        {
            return frameTimeMs.ToString("0.0", CultureInfo.InvariantCulture) + " ms";
        }
    }
}
