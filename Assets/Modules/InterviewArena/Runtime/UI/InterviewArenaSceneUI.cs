using LearningArchitect.Core;
using LearningArchitect.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    public sealed class InterviewArenaSceneUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI subtitleLabel;
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI backButtonLabel;

        private readonly ISceneNavigation sceneNavigation = new SceneNavigationGateway();

        private void Awake()
        {
            if (backButton != null)
                backButton.onClick.AddListener(sceneNavigation.LoadArchitectureShowcase);

            RefreshLabels();
        }

        private void OnEnable()
        {
            ShowcaseLocalization.LanguageChanged += HandleLanguageChanged;
            InterviewArenaRunSession.RunCompleted += HandleRunCompleted;
            RefreshLabels();
        }

        private void OnDisable()
        {
            ShowcaseLocalization.LanguageChanged -= HandleLanguageChanged;
            InterviewArenaRunSession.RunCompleted -= HandleRunCompleted;
        }

        private void HandleRunCompleted()
        {
            if (subtitleLabel == null)
                return;

            ShowcaseLanguage language = ResolveLanguage();
            subtitleLabel.text = language == ShowcaseLanguage.Russian
                ? "Забег завершён. Можно вернуться к архитектурным демо."
                : "Run complete. You can return to the architecture demos.";
        }

        private void HandleLanguageChanged(ShowcaseLanguage language)
        {
            RefreshLabels();
        }

        private void RefreshLabels()
        {
            ShowcaseLanguage language = ResolveLanguage();

            if (titleLabel != null)
                titleLabel.text = ShowcaseLocalizationContent.GetText(language, "interview_arena_title");

            if (subtitleLabel != null)
                subtitleLabel.text = ShowcaseLocalizationContent.GetText(language, "interview_arena_subtitle");

            if (backButtonLabel != null)
                backButtonLabel.text = ShowcaseLocalizationContent.GetText(language, "interview_arena_back");
        }

        private static ShowcaseLanguage ResolveLanguage()
        {
            if (ShowcaseLocalization.Instance != null)
                return ShowcaseLocalization.CurrentLanguage;

            int stored = PlayerPrefs.GetInt(ShowcaseLocalization.PlayerPrefsKey, (int)ShowcaseLanguage.English);
            return stored == (int)ShowcaseLanguage.Russian
                ? ShowcaseLanguage.Russian
                : ShowcaseLanguage.English;
        }
    }
}
