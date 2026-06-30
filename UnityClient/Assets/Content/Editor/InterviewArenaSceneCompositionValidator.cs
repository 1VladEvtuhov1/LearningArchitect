using LearningArchitect.Core;
using LearningArchitect.Modules.InterviewArena;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningArchitect.EditorTools
{
    public static class InterviewArenaSceneCompositionValidator
    {
        private const string ArenaScenePath = ShowcaseSceneNames.InterviewArenaPath;

        private static readonly string[] RequiredRootSections =
        {
            "Level",
            "Gameplay",
            "Actors",
            "Runtime",
            "UI",
            "Cameras",
            "Lighting"
        };

        private static readonly string[] RequiredCanvasNames =
        {
            "Canvas_Static",
            "Canvas_HUD_Dynamic",
            "Canvas_Popups",
            "Canvas_Debug"
        };

        [MenuItem("Learning Architect/Interview Arena/Validate Scene Composition")]
        public static void ValidateSceneCompositionMenu()
        {
            SceneCompositionReport report = ValidateSceneComposition();
            report.LogToConsole();
        }

        public static SceneCompositionReport ValidateSceneComposition()
        {
            var report = new SceneCompositionReport();

            if (!System.IO.File.Exists(ArenaScenePath))
            {
                report.AddError($"Scene not found: {ArenaScenePath}");
                return report;
            }

            Scene existingScene = SceneManager.GetSceneByPath(ArenaScenePath);
            Scene scene = existingScene.IsValid() && existingScene.isLoaded
                ? existingScene
                : EditorSceneManager.OpenScene(ArenaScenePath, OpenSceneMode.Single);

            bool openedForValidation = !existingScene.IsValid() || !existingScene.isLoaded;
            try
            {
                ValidateRootSections(scene, report);
                ValidateBootstrap(scene, report);
                ValidateCombatServices(scene, report);
                ValidatePlayerPool(scene, report);
                ValidatePlayerLocomotionWiring(scene, report);
                ValidateRuntimePlayersSection(scene, report);
                ValidateRuntimeProjectiles(scene, report);
                ValidateUiCanvases(scene, report);
                ValidateUiNaming(scene, report);
                ValidateSceneRootNames(scene, report);
                ValidateInactiveSectionRoots(scene, report);
            }
            finally
            {
                if (openedForValidation && scene.isLoaded)
                    EditorSceneManager.CloseScene(scene, true);
            }

            return report;
        }

        private static void ValidateRootSections(Scene scene, SceneCompositionReport report)
        {
            foreach (string sectionName in RequiredRootSections)
            {
                if (FindSectionRoot(scene, sectionName) == null)
                    report.AddError($"Root section missing: {sectionName}");
            }

            foreach (string sectionName in RequiredRootSections)
            {
                GameObject section = FindSectionRoot(scene, sectionName);
                if (section == null)
                    continue;

                ValidateSectionRootComponents(section, sectionName, report);
            }
        }

        private static void ValidateSectionRootComponents(GameObject section, string sectionName, SceneCompositionReport report)
        {
            MonoBehaviour[] behaviours = section.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null)
                    continue;

                report.AddError(
                    $"Root section '{sectionName}' has component '{behaviour.GetType().Name}' on the section object. Use child systems (e.g. Gameplay/InterviewArenaBootstrap).",
                    section);
            }
        }

        private static void ValidateBootstrap(Scene scene, SceneCompositionReport report)
        {
            InterviewArenaRuntimeContext context = Object.FindFirstObjectByType<InterviewArenaRuntimeContext>();
            if (context == null)
            {
                report.AddError("InterviewArenaRuntimeContext not found in scene.");
                return;
            }

            SerializedObject serialized = new SerializedObject(context);
            if (serialized.FindProperty("player").objectReferenceValue == null)
                report.AddError("InterviewArenaRuntimeContext.player is not assigned.", context);

            if (serialized.FindProperty("combatServices").objectReferenceValue == null)
                report.AddError("InterviewArenaRuntimeContext.combatServices is not assigned.", context);

            if (serialized.FindProperty("arenaFloorCollider").objectReferenceValue == null)
                report.AddError("InterviewArenaRuntimeContext.arenaFloorCollider is not assigned.", context);

            if (serialized.FindProperty("cameraFollow").objectReferenceValue == null)
                report.AddError("InterviewArenaRuntimeContext.cameraFollow is not assigned.", context);

            if (serialized.FindProperty("playerIframeHud").objectReferenceValue == null)
                report.AddError("InterviewArenaRuntimeContext.playerIframeHud is not assigned.", context);

            if (serialized.FindProperty("playerBuffHud").objectReferenceValue == null)
                report.AddError("InterviewArenaRuntimeContext.playerBuffHud is not assigned.", context);

            if (serialized.FindProperty("projectilesRoot").objectReferenceValue == null)
                report.AddError("InterviewArenaRuntimeContext.projectilesRoot is not assigned.", context);

            if (serialized.FindProperty("spawnPoint").objectReferenceValue == null)
                report.AddError("InterviewArenaRuntimeContext.spawnPoint is not assigned.", context);
        }

        private static void ValidateCombatServices(Scene scene, SceneCompositionReport report)
        {
            GameObject gameplay = FindSectionRoot(scene, "Gameplay");
            if (gameplay == null)
                return;

            Transform services = gameplay.transform.Find("ArenaCombatServices");
            if (services == null)
            {
                report.AddError("Gameplay/ArenaCombatServices is missing.");
                return;
            }

            Transform poolHost = services.Find("PlayerCrossbowBoltPool");
            if (poolHost == null)
            {
                report.AddError("Gameplay/ArenaCombatServices/PlayerCrossbowBoltPool is missing.");
                return;
            }

            if (poolHost.GetComponent<ProjectilePool>() == null)
            {
                report.AddError(
                    "PlayerCrossbowBoltPool must have ProjectilePool component.",
                    poolHost.gameObject);
            }
        }

        private static void ValidatePlayerPool(Scene scene, SceneCompositionReport report)
        {
            PlayerMotor player = Object.FindFirstObjectByType<PlayerMotor>();
            if (player == null)
                return;

            ProjectilePool embeddedPool = player.GetComponentInChildren<ProjectilePool>(true);
            if (embeddedPool != null)
            {
                report.AddError(
                    "ProjectilePool found under player instance. Move pool to ArenaCombatServices and bolt instances to Runtime/Projectiles.",
                    embeddedPool);
            }

            Transform boltPoolChild = player.transform.Find("CrossbowBoltPool");
            if (boltPoolChild != null)
            {
                report.AddError(
                    "CrossbowBoltPool child found under player. Remove and use scene ArenaCombatServices.",
                    boltPoolChild.gameObject);
            }
        }

        private static void ValidateRuntimeProjectiles(Scene scene, SceneCompositionReport report)
        {
            GameObject runtime = FindSectionRoot(scene, "Runtime");
            if (runtime == null)
                return;

            Transform projectilesRoot = runtime.transform.Find("Projectiles");
            if (projectilesRoot == null)
            {
                report.AddError("Runtime/Projectiles is missing.");
                return;
            }

            Projectile[] allProjectiles = Object.FindObjectsByType<Projectile>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Projectile projectile in allProjectiles)
            {
                if (projectile == null)
                    continue;

                if (projectile.transform.IsChildOf(projectilesRoot))
                    continue;

                if (projectile.GetComponentInParent<EnemyBrain>() != null)
                    continue;

                report.AddError(
                    $"Projectile '{projectile.name}' is not under Runtime/Projectiles.",
                    projectile);
            }
        }

        private static void ValidatePlayerLocomotionWiring(Scene scene, SceneCompositionReport report)
        {
            PlayerMotor player = Object.FindFirstObjectByType<PlayerMotor>();
            if (player == null)
                return;

            SerializedObject motorSerialized = new SerializedObject(player);
            if (motorSerialized.FindProperty("movementCamera").objectReferenceValue == null)
                report.AddError("PlayerMotor.movementCamera is not assigned.", player);

            if (motorSerialized.FindProperty("viewPivot").objectReferenceValue == null)
                report.AddError("PlayerMotor.viewPivot is not assigned.", player);

            ArenaHoverMotor hover = player.GetComponent<ArenaHoverMotor>();
            if (hover == null)
            {
                report.AddError("Player is missing ArenaHoverMotor.", player);
                return;
            }

            SerializedObject hoverSerialized = new SerializedObject(hover);
            if (hoverSerialized.FindProperty("hoverAnchor").objectReferenceValue == null)
                report.AddError("ArenaHoverMotor.hoverAnchor is not assigned.", hover);

            if (hoverSerialized.FindProperty("inputReader").objectReferenceValue == null)
                report.AddError("ArenaHoverMotor.inputReader is not assigned.", hover);

            ArenaCursorAim aim = player.GetComponent<ArenaCursorAim>();
            if (aim == null)
            {
                report.AddError("Player is missing ArenaCursorAim.", player);
                return;
            }

            SerializedObject aimSerialized = new SerializedObject(aim);
            if (aimSerialized.FindProperty("aimPivot").objectReferenceValue == null)
                report.AddError("ArenaCursorAim.aimPivot is not assigned.", aim);

            if (aimSerialized.FindProperty("targetCamera").objectReferenceValue == null)
                report.AddError("ArenaCursorAim.targetCamera is not assigned.", aim);

            InterviewArenaRuntimeContext context = Object.FindFirstObjectByType<InterviewArenaRuntimeContext>();
            if (context == null)
                return;

            SerializedObject contextSerialized = new SerializedObject(context);
            InterviewArenaCameraFollow follow =
                contextSerialized.FindProperty("cameraFollow").objectReferenceValue as InterviewArenaCameraFollow;
            if (follow == null)
                return;

            SerializedObject followSerialized = new SerializedObject(follow);
            if (followSerialized.FindProperty("target").objectReferenceValue == null)
                report.AddError("InterviewArenaCameraFollow.target is not assigned.", follow);
        }

        private static void ValidateRuntimePlayersSection(Scene scene, SceneCompositionReport report)
        {
            GameObject runtime = FindSectionRoot(scene, "Runtime");
            if (runtime == null)
                return;

            Transform players = runtime.transform.Find("Players");
            if (players != null)
            {
                report.AddWarning(
                    "Runtime/Players exists. Per SCENE_COMPOSITION.md, players/enemies should live under Actors (Runtime is for projectiles/VFX/pickups/temporary objects).",
                    players.gameObject);
            }
        }

        private static void ValidateUiCanvases(Scene scene, SceneCompositionReport report)
        {
            GameObject uiRoot = FindSectionRoot(scene, "UI");
            if (uiRoot == null)
                return;

            foreach (string canvasName in RequiredCanvasNames)
            {
                if (uiRoot.transform.Find(canvasName) == null)
                    report.AddWarning($"UI/{canvasName} is missing (target layout per SCENE_COMPOSITION.md).");
            }
        }

        private static void ValidateUiNaming(Scene scene, SceneCompositionReport report)
        {
            TextMeshProUGUI[] labels = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (TextMeshProUGUI label in labels)
            {
                if (label == null)
                    continue;

                string name = label.gameObject.name;
                if (IsDefaultUiName(name))
                {
                    report.AddWarning($"Text object uses default name: {GetHierarchyPath(label.transform)}", label);
                    continue;
                }

                if (!LooksLikeUiWidgetName(name) && name is not "Text - Label")
                    report.AddWarning($"TMP object should use 'Text - Name' format: {GetHierarchyPath(label.transform)}", label);
            }

            Image[] images = Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Image image in images)
            {
                if (image == null || image.GetComponent<Button>() != null)
                    continue;

                string name = image.gameObject.name;
                if (IsDefaultUiName(name))
                    report.AddWarning($"Image uses default name: {GetHierarchyPath(image.transform)}", image);
            }
        }

        private static void ValidateSceneRootNames(Scene scene, SceneCompositionReport report)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name.Contains("(Clone)"))
                    report.AddError($"Root object uses clone name: {root.name}", root);

                if (root.name is "GameObject" or "Cube" or "New Text")
                    report.AddError($"Root object uses placeholder name: {root.name}", root);
            }
        }

        private static void ValidateInactiveSectionRoots(Scene scene, SceneCompositionReport report)
        {
            foreach (string sectionName in new[] { "Runtime", "UI", "Level" })
            {
                GameObject section = FindSectionRoot(scene, sectionName);
                if (section != null && !section.activeSelf)
                {
                    report.AddWarning(
                        $"Root section '{sectionName}' is inactive. Avoid toggling whole sections.",
                        section);
                }
            }
        }

        private static void ValidateBadName(GameObject target, SceneCompositionReport report)
        {
            if (target == null)
                return;

            string name = target.name;
            if (name is "GameObject" or "New Text" or "Cube")
                report.AddWarning($"Placeholder object name: {GetHierarchyPath(target.transform)}", target);
        }

        private static GameObject FindSectionRoot(Scene scene, string sectionName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                if (root.name == sectionName)
                    return root;

                Transform nested = root.transform.Find(sectionName);
                if (nested != null)
                    return nested.gameObject;
            }

            return null;
        }

        private static bool IsDefaultUiName(string name) =>
            name is "Text" or "Label" or "Background" or "Fill" or "Title" or "Subtitle" or "New Text";

        private static bool LooksLikeUiWidgetName(string name)
        {
            int dash = name.IndexOf(" - ");
            if (dash <= 0)
                return false;

            string prefix = name.Substring(0, dash);
            return prefix is "Container" or "Button" or "Image" or "Text" or "Slider" or "Toggle" or "Input";
        }

        private static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
                return string.Empty;

            var path = new StringBuilder(transform.name);
            Transform current = transform.parent;
            while (current != null)
            {
                path.Insert(0, current.name + "/");
                current = current.parent;
            }

            return path.ToString();
        }

        public sealed class SceneCompositionReport
        {
            private readonly List<string> errors = new();
            private readonly List<string> warnings = new();

            public IReadOnlyList<string> Errors => errors;
            public IReadOnlyList<string> Warnings => warnings;
            public bool Succeeded => errors.Count == 0;

            public void AddError(string message, Object context = null)
            {
                errors.Add(message);
                if (context != null)
                    Debug.LogError("[InterviewArena Composition] ERROR: " + message, context);
            }

            public void AddWarning(string message, Object context = null)
            {
                warnings.Add(message);
                if (context != null)
                    Debug.LogWarning("[InterviewArena Composition] WARN: " + message, context);
            }

            public void LogToConsole()
            {
                var summary = new StringBuilder();
                summary.AppendLine($"[InterviewArena] Scene composition: {errors.Count} error(s), {warnings.Count} warning(s).");

                foreach (string error in errors)
                    summary.AppendLine("ERROR: " + error);

                foreach (string warning in warnings)
                    summary.AppendLine("WARN: " + warning);

                if (Succeeded && warnings.Count == 0)
                    Debug.Log(summary.ToString());
                else if (Succeeded)
                    Debug.LogWarning(summary.ToString());
                else
                    Debug.LogError(summary.ToString());
            }
        }
    }
}
