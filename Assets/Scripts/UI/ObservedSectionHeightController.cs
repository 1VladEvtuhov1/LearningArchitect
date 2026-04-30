using System;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.UI
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    [AddComponentMenu("Layout/Observed Section Height Controller")]
    public sealed class ObservedSectionHeightController : MonoBehaviour
    {
        [Serializable]
        public sealed class ObservedSectionEntry
        {
            [SerializeField] private RectTransform rect;
            [SerializeField] private float extraHeight;

            public RectTransform Rect => rect;

            public float ExtraHeight => extraHeight;

            internal float BaselineHeight { get; set; }

            internal float LastRectHeight { get; set; }
        }

        [Serializable]
        public sealed class DependentSection
        {
            [SerializeField] private RectTransform rect;
            [SerializeField] private bool shiftVertically = true;
            [SerializeField] private bool shrinkHeightByObservedDelta;
            [SerializeField] private float minHeight;

            public RectTransform Rect => rect;

            public bool ShiftVertically => shiftVertically;

            public bool ShrinkHeightByObservedDelta => shrinkHeightByObservedDelta;

            public float MinHeight => minHeight;

            internal Vector2 BaselineAnchoredPosition { get; set; }

            internal Vector2 BaselineSizeDelta { get; set; }
        }

        [Header("Observed Sections")]
        [SerializeField] private ObservedSectionEntry[] observedSections = Array.Empty<ObservedSectionEntry>();

        [Header("Legacy Single Observed Section")]
        [SerializeField] private RectTransform observedSection;
        [SerializeField] private float extraObservedHeight;

        [Header("Refresh")]
        [SerializeField] private bool roundToWholePixels = true;
        [SerializeField] private bool autoRefreshWhenObservedRectChanges;

        [Header("Dependents")]
        [SerializeField] private DependentSection[] dependentSections = Array.Empty<DependentSection>();

        private bool baselineCaptured;
        private float lastAppliedHeightDelta = float.NaN;
        private float legacyBaselineHeight;
        private float legacyLastRectHeight = float.NaN;
        private bool refreshQueued;

        private void Awake()
        {
            EnsureSerializedReferences();
            RecaptureBaseline();
        }

        private void OnEnable()
        {
            RequestRefresh();
        }

        private void OnValidate()
        {
            if (observedSections == null)
                observedSections = Array.Empty<ObservedSectionEntry>();

            CompactObservedSections();

            if (dependentSections == null)
            {
                dependentSections = Array.Empty<DependentSection>();
                return;
            }

            CompactDependentSections();
        }

        public void RequestRefresh()
        {
            if (refreshQueued)
                return;

            refreshQueued = true;

            if (!Application.isPlaying)
                RefreshImmediate();
        }

        public void RefreshImmediate()
        {
            refreshQueued = false;
            EnsureBaselineCaptured();
            ApplyObservedHeight();
        }

        public void RecaptureBaseline()
        {
            EnsureSerializedReferences();
            CaptureBaseline();
        }

        public void RecaptureBaselineAndRefresh()
        {
            RecaptureBaseline();
            RefreshImmediate();
        }

        private void LateUpdate()
        {
            if (autoRefreshWhenObservedRectChanges)
                TryAutoRefresh();

            if (!refreshQueued)
                return;

            RefreshImmediate();
        }

        private void CompactObservedSections()
        {
            int validCount = 0;
            for (int i = 0; i < observedSections.Length; i++)
            {
                if (observedSections[i] != null)
                    validCount++;
            }

            if (validCount == observedSections.Length)
                return;

            ObservedSectionEntry[] compacted = new ObservedSectionEntry[validCount];
            int targetIndex = 0;
            for (int i = 0; i < observedSections.Length; i++)
            {
                if (observedSections[i] == null)
                    continue;

                compacted[targetIndex++] = observedSections[i];
            }

            observedSections = compacted;
        }

        private void CompactDependentSections()
        {
            int validCount = 0;
            for (int i = 0; i < dependentSections.Length; i++)
            {
                if (dependentSections[i] != null)
                    validCount++;
            }

            if (validCount == dependentSections.Length)
                return;

            DependentSection[] compacted = new DependentSection[validCount];
            int targetIndex = 0;
            for (int i = 0; i < dependentSections.Length; i++)
            {
                if (dependentSections[i] == null)
                    continue;

                compacted[targetIndex++] = dependentSections[i];
            }

            dependentSections = compacted;
        }

        private void EnsureBaselineCaptured()
        {
            if (baselineCaptured)
                return;

            CaptureBaseline();
        }

        private void CaptureBaseline()
        {
            for (int i = 0; i < observedSections.Length; i++)
                CaptureObservedBaseline(observedSections[i]);

            if (!HasConfiguredObservedSections() && observedSection != null)
            {
                RebuildLegacyObservedSectionIfActive();
                legacyBaselineHeight = MeasureObservedHeight(observedSection, extraObservedHeight);
                legacyLastRectHeight = ReadObservedRectHeight(observedSection);
            }

            for (int i = 0; i < dependentSections.Length; i++)
            {
                DependentSection section = dependentSections[i];
                section.BaselineAnchoredPosition = section.Rect.anchoredPosition;
                section.BaselineSizeDelta = section.Rect.sizeDelta;
            }

            baselineCaptured = true;
            lastAppliedHeightDelta = float.NaN;
        }

        private void ApplyObservedHeight()
        {
            float heightDelta = 0f;

            for (int i = 0; i < observedSections.Length; i++)
                heightDelta += MeasureObservedDelta(observedSections[i]);

            if (!HasConfiguredObservedSections() && observedSection != null)
                heightDelta += MeasureLegacyObservedDelta();

            if (roundToWholePixels)
                heightDelta = Mathf.Round(heightDelta);

            if (Approximately(lastAppliedHeightDelta, heightDelta))
                return;

            for (int i = 0; i < dependentSections.Length; i++)
                ApplySection(heightDelta, dependentSections[i]);

            lastAppliedHeightDelta = heightDelta;
        }

        private void ApplySection(float heightDelta, DependentSection section)
        {
            if (section.Rect == null)
                return;

            Vector2 nextAnchoredPosition = section.BaselineAnchoredPosition;
            if (section.ShiftVertically)
                nextAnchoredPosition.y = section.BaselineAnchoredPosition.y - heightDelta;

            if (!Approximately(section.Rect.anchoredPosition, nextAnchoredPosition))
                section.Rect.anchoredPosition = nextAnchoredPosition;

            if (!section.ShrinkHeightByObservedDelta)
                return;

            Vector2 nextSizeDelta = section.BaselineSizeDelta;
            nextSizeDelta.y = Mathf.Max(section.MinHeight, section.BaselineSizeDelta.y - heightDelta);

            if (roundToWholePixels)
                nextSizeDelta.y = Mathf.Round(nextSizeDelta.y);

            if (!Approximately(section.Rect.sizeDelta, nextSizeDelta))
                section.Rect.sizeDelta = nextSizeDelta;
        }

        private void CaptureObservedBaseline(ObservedSectionEntry section)
        {
            if (section == null || section.Rect == null)
                return;

            RebuildObservedSectionIfActive(section.Rect);
            section.BaselineHeight = MeasureObservedHeight(section.Rect, section.ExtraHeight);
            section.LastRectHeight = ReadObservedRectHeight(section.Rect);
        }

        private void TryAutoRefresh()
        {
            for (int i = 0; i < observedSections.Length; i++)
            {
                ObservedSectionEntry section = observedSections[i];
                if (section == null || section.Rect == null)
                    continue;

                float currentRectHeight = ReadObservedRectHeight(section.Rect);
                if (Approximately(section.LastRectHeight, currentRectHeight))
                    continue;

                RequestRefresh();
                return;
            }

            if (HasConfiguredObservedSections() || observedSection == null)
                return;

            float legacyRectHeight = ReadObservedRectHeight(observedSection);
            if (!Approximately(legacyLastRectHeight, legacyRectHeight))
                RequestRefresh();
        }

        private float MeasureObservedDelta(ObservedSectionEntry section)
        {
            if (section == null || section.Rect == null)
                return 0f;

            RebuildObservedSectionIfActive(section.Rect);
            float observedHeight = MeasureObservedHeight(section.Rect, section.ExtraHeight);
            section.LastRectHeight = ReadObservedRectHeight(section.Rect);
            return observedHeight - section.BaselineHeight;
        }

        private float MeasureLegacyObservedDelta()
        {
            RebuildLegacyObservedSectionIfActive();
            float observedHeight = MeasureObservedHeight(observedSection, extraObservedHeight);
            legacyLastRectHeight = ReadObservedRectHeight(observedSection);
            return observedHeight - legacyBaselineHeight;
        }

        private void EnsureSerializedReferences()
        {
            if (!HasConfiguredObservedSections() && observedSection == null)
                throw new InvalidOperationException("ObservedSectionHeightController: no observed sections are assigned.");

            for (int i = 0; i < observedSections.Length; i++)
            {
                if (observedSections[i] == null)
                    throw new InvalidOperationException($"ObservedSectionHeightController: observed section #{i} is null.");

                if (observedSections[i].Rect == null)
                    throw new InvalidOperationException($"ObservedSectionHeightController: observed section #{i} rect is not assigned.");
            }

            if (dependentSections == null || dependentSections.Length == 0)
                throw new InvalidOperationException("ObservedSectionHeightController: dependentSections are not assigned.");

            for (int i = 0; i < dependentSections.Length; i++)
            {
                if (dependentSections[i] == null)
                    throw new InvalidOperationException($"ObservedSectionHeightController: dependent section #{i} is null.");

                if (dependentSections[i].Rect == null)
                    throw new InvalidOperationException($"ObservedSectionHeightController: dependent section #{i} rect is not assigned.");
            }
        }

        private bool HasConfiguredObservedSections()
        {
            return observedSections != null && observedSections.Length > 0;
        }

        private void RebuildLegacyObservedSectionIfActive()
        {
            RebuildObservedSectionIfActive(observedSection);
        }

        private void RebuildObservedSectionIfActive(RectTransform rect)
        {
            if (rect == null || !rect.gameObject.activeInHierarchy)
                return;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }

        private float MeasureObservedHeight(RectTransform rect, float extraHeight)
        {
            float preferredHeight = LayoutUtility.GetPreferredHeight(rect);
            float minHeight = LayoutUtility.GetMinHeight(rect);
            float measuredHeight = Mathf.Max(rect.rect.height, preferredHeight, minHeight);
            return Mathf.Max(0f, measuredHeight + extraHeight);
        }

        private float ReadObservedRectHeight(RectTransform rect)
        {
            return rect != null ? rect.rect.height : 0f;
        }

        private static bool Approximately(Vector2 left, Vector2 right)
        {
            return Mathf.Abs(left.x - right.x) < 0.01f &&
                   Mathf.Abs(left.y - right.y) < 0.01f;
        }

        private static bool Approximately(float left, float right)
        {
            return Mathf.Abs(left - right) < 0.01f;
        }
    }
}
