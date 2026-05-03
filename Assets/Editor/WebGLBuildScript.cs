using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LearningArchitect.Editor
{
    public static class WebGLBuildScript
    {
        private const string ShowcaseScenePath = "Assets/Showcase/Scenes/ArchitectureShowcase.unity";
        private const string OutputRoot = "Builds/WebGL";

        [MenuItem("Tools/LearningArchitect/Build WebGL Site Export")]
        public static void BuildWebGlSiteExportMenu()
        {
            BuildWebGlSiteExport();
        }

        public static void BuildWebGlSiteExport()
        {
            if (!File.Exists(ShowcaseScenePath))
                throw new FileNotFoundException("Showcase scene was not found.", ShowcaseScenePath);

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string outputPath = Path.GetFullPath(Path.Combine(projectRoot, OutputRoot));

            Directory.CreateDirectory(outputPath);

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { ShowcaseScenePath },
                target = BuildTarget.WebGL,
                locationPathName = outputPath,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"WebGL build failed. Result={summary.result}; errors={summary.totalErrors}; warnings={summary.totalWarnings}.");
            }

            UnityEngine.Debug.Log(
                $"WebGL build completed at '{outputPath}'. Size={summary.totalSize} bytes; warnings={summary.totalWarnings}.");
        }
    }
}
