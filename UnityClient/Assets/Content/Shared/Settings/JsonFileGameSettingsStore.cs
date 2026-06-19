using System;
using System.IO;
using UnityEngine;

namespace LearningArchitect.Shared.Settings
{
    public static class JsonFileGameSettingsStore
    {
        public const string FileName = "game_settings.json";

        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static bool Exists() => File.Exists(FilePath);

        public static bool TryLoad(out GameSettingsData data)
        {
            data = GameSettingsData.CreateDefault();
            if (!File.Exists(FilePath))
                return false;

            try
            {
                string json = File.ReadAllText(FilePath);
                if (string.IsNullOrWhiteSpace(json))
                    return false;

                GameSettingsData parsed = JsonUtility.FromJson<GameSettingsData>(json);
                if (parsed == null)
                    return false;

                data = parsed;
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[GameSettings] Failed to load settings: " + exception.Message);
                return false;
            }
        }

        public static bool TrySave(GameSettingsData data)
        {
            if (data == null)
                return false;

            try
            {
                string directory = Application.persistentDataPath;
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                data.schemaVersion = GameSettingsData.SchemaVersion;
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(FilePath, json);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[GameSettings] Failed to save settings: " + exception.Message);
                return false;
            }
        }
    }
}
