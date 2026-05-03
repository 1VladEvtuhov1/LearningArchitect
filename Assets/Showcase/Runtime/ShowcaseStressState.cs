using System;

namespace LearningArchitect.Core
{
    public sealed class ShowcaseStressState
    {
        private int activeItemCount;
        private int currentStressLevel = 1000;
        private int selectedPresetIndex = -1;
        private ShowcaseMetricsSnapshot metricsSnapshot = ShowcaseMetricsSnapshot.Empty;

        public int CurrentStressLevel => currentStressLevel;

        public int ActiveItemCount => activeItemCount;

        public int SelectedPresetIndex => selectedPresetIndex;

        public ShowcaseMetricsSnapshot MetricsSnapshot => metricsSnapshot;

        public void SetStressLevel(int count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count));

            currentStressLevel = count;
        }

        public void SetSelectedPresetIndex(int presetIndex)
        {
            if (presetIndex < -1)
                throw new ArgumentOutOfRangeException(nameof(presetIndex));

            selectedPresetIndex = presetIndex;
        }

        public void SetActiveItemCount(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            activeItemCount = count;
        }

        public void SetMetricsSnapshot(ShowcaseMetricsSnapshot snapshot)
        {
            metricsSnapshot = snapshot;
            activeItemCount = snapshot.HasVisibleCount ? snapshot.VisibleCount : 0;
        }
    }
}
