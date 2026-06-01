using UnityEditor;
using UnityEngine;

namespace LearningArchitect.Editor
{
    public static class ShowcaseLayoutTool
    {
        private const string SafeModeMessage = "Showcase layout rebuild is deprecated and disabled. Use Assets/Content/Showcase/Prefabs/ArchitectureShowcaseHub.prefab as the source of truth, or inspect Assets/Content/Showcase/Prefabs/Archive/ArchitectureShowcaseHub_LegacyArchive.prefab as an archived baseline until a new explicit tool is designed.";

        [MenuItem("Tools/LearningArchitect/Rebuild Showcase Layout")]
        public static void RebuildMenu()
        {
            Debug.LogWarning(SafeModeMessage);
        }

        [MenuItem("Tools/LearningArchitect/Apply Showcase Layout To Scene")]
        public static void ApplyToSceneMenu()
        {
            Debug.LogWarning(SafeModeMessage);
        }

        public static string Rebuild()
        {
            Debug.LogWarning(SafeModeMessage);
            return SafeModeMessage;
        }

        public static string ApplyToScene()
        {
            Debug.LogWarning(SafeModeMessage);
            return SafeModeMessage;
        }
    }
}
