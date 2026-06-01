using LearningArchitect.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.UI
{
    /// <summary>
    /// Bottom dock on the architecture showcase hub that opens the separate Interview Arena scene.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InterviewArenaLaunchDock : MonoBehaviour
    {
        private const float DockHeight = 52f;

        [SerializeField] private RectTransform dockRoot;
        [SerializeField] private Button launchButton;
        [SerializeField] private TextMeshProUGUI launchLabel;

        private void Awake()
        {
            if (launchButton == null)
                BuildDock();

            launchButton.onClick.AddListener(ShowcaseSceneLoader.LoadInterviewArena);
            RefreshLabel();
            ShowcaseLocalization.LanguageChanged += HandleLanguageChanged;
        }

        private void OnDestroy()
        {
            ShowcaseLocalization.LanguageChanged -= HandleLanguageChanged;
        }

        private void HandleLanguageChanged(ShowcaseLanguage language)
        {
            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (launchLabel == null)
                return;

            launchLabel.text = ShowcaseLocalization.GetText("interview_arena_launch");
        }

        private void BuildDock()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError($"{nameof(InterviewArenaLaunchDock)} requires a parent Canvas.");
                return;
            }

            GameObject dockObject = new GameObject("InterviewArenaLaunchDock", typeof(RectTransform));
            dockObject.transform.SetParent(canvas.transform, false);
            dockRoot = dockObject.GetComponent<RectTransform>();
            dockRoot.anchorMin = new Vector2(0f, 0f);
            dockRoot.anchorMax = new Vector2(1f, 0f);
            dockRoot.pivot = new Vector2(0.5f, 0f);
            dockRoot.sizeDelta = new Vector2(0f, DockHeight);
            dockRoot.anchoredPosition = Vector2.zero;

            Image background = dockObject.AddComponent<Image>();
            background.color = ShowcasePalette.WithAlpha(ShowcasePalette.PanelMain, 0.92f);
            background.raycastTarget = true;

            GameObject buttonObject = new GameObject("LaunchInterviewArenaButton", typeof(RectTransform));
            buttonObject.transform.SetParent(dockRoot, false);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(320f, 36f);
            buttonRect.anchoredPosition = Vector2.zero;

            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = ShowcasePalette.AccentSoft(0.35f);

            launchButton = buttonObject.AddComponent<Button>();
            launchButton.targetGraphic = buttonImage;

            GameObject labelObject = new GameObject("Label", typeof(RectTransform));
            labelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            launchLabel = labelObject.AddComponent<TextMeshProUGUI>();
            launchLabel.alignment = TextAlignmentOptions.Center;
            launchLabel.fontSize = 15f;
            launchLabel.color = ShowcasePalette.TextPrimary;
            launchLabel.text = "Interview Arena";
        }
    }
}
