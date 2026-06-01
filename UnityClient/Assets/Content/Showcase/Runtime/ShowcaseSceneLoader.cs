using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningArchitect.Core
{
    public static class ShowcaseSceneLoader
    {
        public static void LoadArchitectureShowcase()
        {
            LoadByName(ShowcaseSceneNames.ArchitectureShowcase);
        }

        public static void LoadInterviewArena()
        {
            LoadByName(ShowcaseSceneNames.InterviewArena);
        }

        private static void LoadByName(string sceneName)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError(
                    $"[ShowcaseSceneLoader] Scene '{sceneName}' is not in the build settings. " +
                    "Run Learning Architect/Interview Arena/Setup Interview Arena Scenes.");
                return;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
