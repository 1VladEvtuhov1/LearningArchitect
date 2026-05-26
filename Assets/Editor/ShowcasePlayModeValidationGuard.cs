using UnityEditor;
using UnityEngine;
using LearningArchitect.Core;

namespace LearningArchitect.EditorTools
{
    [InitializeOnLoad]
    public static class ShowcasePlayModeValidationGuard
    {
        static ShowcasePlayModeValidationGuard()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode)
                return;

            ModuleDefinitionSO[] modules = LoadAssets<ModuleDefinitionSO>();
            VariantDefinitionSO[] variants = LoadAssets<VariantDefinitionSO>();
            RecruiterDemoScenarioSO[] scenarios = LoadAssets<RecruiterDemoScenarioSO>();
            ShowcaseValidationReport report = ShowcaseValidator.ValidateShowcaseConfigurationInternal(modules, variants, scenarios);
            if (report.IsValid)
                return;

            ShowcaseValidator.LogReport(report, modules.Length, variants.Length);
            EditorApplication.isPlaying = false;
            Debug.LogError("[ShowcaseValidator] Play mode entry was cancelled because showcase validation reported errors (see messages above). You can also run Tools/LearningArchitect/Validate Showcase Configuration.");
        }

        private static T[] LoadAssets<T>()
            where T : Object
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { "Assets/Showcase/Data", "Assets/Modules" });
            T[] assets = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                assets[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return assets;
        }
    }
}
