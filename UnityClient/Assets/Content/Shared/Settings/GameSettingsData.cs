using System;

namespace LearningArchitect.Shared.Settings
{
    [Serializable]
    public sealed class GameSettingsData
    {
        public const int SchemaVersion = 1;
        public const int LanguageEnglish = 0;
        public const int LanguageRussian = 1;

        public int schemaVersion = SchemaVersion;
        public int language = LanguageEnglish;
        public string lastUsername = string.Empty;
        public float masterVolume = 1f;

        public static GameSettingsData CreateDefault() => new GameSettingsData();
    }
}
