using LearningArchitect.Shared.Settings;
using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class GameSettingsStoreTests
    {
        [Test]
        public void Serializes_settings_json_round_trip()
        {
            GameSettingsData source = new GameSettingsData
            {
                language = GameSettingsData.LanguageRussian,
                lastUsername = "Runner",
                masterVolume = 0.65f,
            };

            string json = JsonUtility.ToJson(source);
            GameSettingsData parsed = JsonUtility.FromJson<GameSettingsData>(json);

            Assert.AreEqual(GameSettingsData.LanguageRussian, parsed.language);
            Assert.AreEqual("Runner", parsed.lastUsername);
            Assert.AreEqual(0.65f, parsed.masterVolume);
        }

        [Test]
        public void Default_settings_use_schema_version()
        {
            GameSettingsData defaults = GameSettingsData.CreateDefault();
            Assert.AreEqual(GameSettingsData.SchemaVersion, defaults.schemaVersion);
            Assert.AreEqual(GameSettingsData.LanguageEnglish, defaults.language);
        }
    }
}
