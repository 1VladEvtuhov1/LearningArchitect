using UnityEditor;
using UnityEditor.Localization;

namespace LearningArchitect.EditorTools
{
    /// <summary>
    /// Unity 6 + com.unity.localization 1.5.x can throw in GameViewLanguageMenu when
    /// EditorStyles.popup is not ready during Play Mode transitions.
    /// </summary>
    [InitializeOnLoad]
    internal static class LocalizationGameViewMenuWorkaround
    {
        private const string DisabledByProjectKey = "LearningArchitect.LocalizationGameViewMenuDisabled";

        static LocalizationGameViewMenuWorkaround()
        {
            if (EditorPrefs.GetBool(DisabledByProjectKey, false))
                return;

            if (!LocalizationEditorSettings.ShowLocaleMenuInGameView)
                return;

            LocalizationEditorSettings.ShowLocaleMenuInGameView = false;
            EditorPrefs.SetBool(DisabledByProjectKey, true);
            UnityEngine.Debug.Log(
                "[LearningArchitect] Disabled Localization 'Locale Game View Menu' to avoid a Unity editor crash on Play Mode. "
                + "Re-enable via Edit > Preferences > Localization if you need in-GameView locale switching.");
        }
    }
}
