using UnityEngine;

namespace LearningArchitect.Shared.Settings
{
    /// <summary>
    /// Cached game settings persisted under <see cref="Application.persistentDataPath"/>.
    /// Migrates legacy PlayerPrefs keys on first load.
    /// </summary>
    public static class GameSettings
    {
        private const string LegacyLanguageKey = "ShowcaseLanguage";
        private const string LegacyUsernameKey = "ExtractionRpg.Username";

        private static GameSettingsData current;
        private static bool loaded;

        public static GameSettingsData Current
        {
            get
            {
                EnsureLoaded();
                return current;
            }
        }

        public static string FilePath => JsonFileGameSettingsStore.FilePath;

        public static void EnsureLoaded()
        {
            if (loaded)
                return;

            current = GameSettingsData.CreateDefault();
            if (!JsonFileGameSettingsStore.TryLoad(out GameSettingsData fromFile))
                MigrateLegacyPlayerPrefs(current);
            else
                current = fromFile;

            loaded = true;
        }

        public static void Reload()
        {
            loaded = false;
            EnsureLoaded();
        }

        public static void Save()
        {
            EnsureLoaded();
            JsonFileGameSettingsStore.TrySave(current);
        }

        public static void SetLanguage(int language)
        {
            EnsureLoaded();
            current.language = language;
            Save();
        }

        public static void SetLastUsername(string username)
        {
            EnsureLoaded();
            current.lastUsername = username ?? string.Empty;
            Save();
        }

        public static void SetMasterVolume(float volume)
        {
            EnsureLoaded();
            current.masterVolume = Mathf.Clamp01(volume);
            Save();
        }

        private static void MigrateLegacyPlayerPrefs(GameSettingsData target)
        {
            if (PlayerPrefs.HasKey(LegacyLanguageKey))
                target.language = PlayerPrefs.GetInt(LegacyLanguageKey, GameSettingsData.LanguageEnglish);

            if (PlayerPrefs.HasKey(LegacyUsernameKey))
                target.lastUsername = PlayerPrefs.GetString(LegacyUsernameKey, string.Empty);

            if (PlayerPrefs.HasKey(LegacyLanguageKey) || PlayerPrefs.HasKey(LegacyUsernameKey))
                JsonFileGameSettingsStore.TrySave(target);
        }
    }
}
