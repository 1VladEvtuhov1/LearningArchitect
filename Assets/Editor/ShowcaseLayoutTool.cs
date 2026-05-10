using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.Editor
{
    public static class ShowcaseLayoutTool
    {
        private const string SafeModeMessage = "Showcase layout rebuild is disabled in safe mode. Use the PreReorganize visual baseline and adjust wiring manually until the tool is rewritten.";

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

        private static void ApplyLayout(GameObject prefabRoot)
        {
            if (prefabRoot == null)
                return;

            Canvas canvas = FindCanvasByChild(prefabRoot.transform, "Container - Root");
            if (canvas == null)
                return;

            Transform rootFrame = FindDeep(canvas.transform, "Container - Root");
            if (rootFrame == null)
                return;

            NormalizeModuleInfoPanel(rootFrame);
            NormalizeDescriptionPanel(rootFrame);
            NormalizeStressControls(rootFrame);
            NormalizeNavigationControls(rootFrame);
        }

        private static void NormalizeModuleInfoPanel(Transform rootFrame)
        {
            RectTransform moduleInfoPanel = FindDeepAny(rootFrame, "Container - ModuleInfo", "ModuleInfoPanel", "HeaderPanel") as RectTransform;
            if (moduleInfoPanel == null)
                return;

            moduleInfoPanel.name = "Container - ModuleInfo";
            EnsureTextChild(moduleInfoPanel, "Text - InputHints", "Realtime architecture preview");
        }

        private static void NormalizeDescriptionPanel(Transform rootFrame)
        {
            RectTransform descriptionPanel = FindDeep(rootFrame, "Scroll - Description") as RectTransform
                ?? FindDeep(rootFrame, "DescriptionPanel") as RectTransform;
            if (descriptionPanel == null)
                return;

            NormalizeTabsBar(descriptionPanel);

            RectTransform viewport = FindDeep(descriptionPanel, "Container - Viewport") as RectTransform
                ?? FindDeep(descriptionPanel, "Viewport") as RectTransform;
            if (viewport == null)
                return;

            ScrollRect scrollRect = descriptionPanel.GetComponent<ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.viewport = viewport;
                scrollRect.content = FindDeep(viewport, "Text - Description") as RectTransform
                    ?? FindDeep(viewport, "DescriptionText") as RectTransform;
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
                scrollRect.movementType = ScrollRect.MovementType.Clamped;
            }

            EnsureTextChild(viewport, "Text - Description", "Description placeholder");
        }

        private static void NormalizeTabsBar(RectTransform descriptionPanel)
        {
            RectTransform tabsBar = FindDeepAny(descriptionPanel, "Layout - DescriptionTabs", "Container - DescriptionCharacters", "TabsBar") as RectTransform;
            if (tabsBar == null)
                return;

            tabsBar.name = "Layout - DescriptionTabs";
            EnsureRectChild(tabsBar, "Image - ActiveTabUnderline");
        }

        private static void NormalizeStressControls(Transform rootFrame)
        {
            Transform stressSection = FindDeep(rootFrame, "Container - StressSection");
            if (stressSection == null)
                return;

            RectTransform stressContent = FindDeep(stressSection, "Container - StressContent") as RectTransform;
            if (stressContent == null)
                stressContent = EnsureRectChild(stressSection, "Container - StressContent");

            Transform legacySummary = FindDirectChild(stressSection, "Container - StressSummary");
            Transform canonicalSummary = FindDirectChild(stressContent, "Container - StressSummary");

            if (canonicalSummary == null && legacySummary != null)
            {
                legacySummary.SetParent(stressContent, false);
                canonicalSummary = legacySummary;
            }
            else if (canonicalSummary != null && legacySummary != null)
            {
                Object.DestroyImmediate(legacySummary.gameObject);
            }

            if (canonicalSummary == null)
                canonicalSummary = EnsureRectChild(stressContent, "Container - StressSummary");

            EnsureTextChild(canonicalSummary, "Text - StressSummary", "Stress summary");
        }

        private static void NormalizeNavigationControls(Transform rootFrame)
        {
            NormalizeGlow(FindDeep(rootFrame, "Button - NextModule") ?? FindDeep(rootFrame, "NextModuleButton"));
            NormalizeGlow(FindDeep(rootFrame, "Button - PrevModule") ?? FindDeep(rootFrame, "PreviousModuleButton"));
        }

        private static void NormalizeGlow(Transform button)
        {
            if (button == null)
                return;

            Transform canonicalGlow = FindDirectChild(button, "Image - ActiveGlow");
            Transform legacyGlow = FindDirectChild(button, "ActiveGlow");

            if (canonicalGlow == null && legacyGlow != null)
            {
                legacyGlow.name = "Image - ActiveGlow";
                canonicalGlow = legacyGlow;
                legacyGlow = null;
            }

            if (canonicalGlow != null && legacyGlow != null)
                Object.DestroyImmediate(legacyGlow.gameObject);
        }

        private static void EnsureTextChild(Transform parent, string name, string defaultText)
        {
            GameObject textObject = EnsureChild(parent, name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            TextMeshProUGUI text = GetOrAdd<TextMeshProUGUI>(textObject);
            if (string.IsNullOrWhiteSpace(text.text))
                text.text = defaultText;

            text.raycastTarget = false;
        }

        private static GameObject EnsureChild(Transform parent, string name, params System.Type[] components)
        {
            Transform child = parent.Find(name);
            if (child != null)
                return child.gameObject;

            GameObject go = new GameObject(name, components);
            go.transform.SetParent(parent, false);
            return go;
        }

        private static RectTransform EnsureRectChild(Transform parent, string name)
        {
            return EnsureChild(parent, name, typeof(RectTransform)).GetComponent<RectTransform>();
        }

        private static T GetOrAdd<T>(GameObject gameObject)
            where T : Component
        {
            T component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private static Canvas FindCanvasByChild(Transform root, string childName)
        {
            Canvas[] canvases = root.GetComponentsInChildren<Canvas>(true);
            for (int i = 0; i < canvases.Length; i++)
            {
                if (FindDeep(canvases[i].transform, childName) != null)
                    return canvases[i];
            }

            return null;
        }

        private static Transform FindDirectChild(Transform parent, string name)
        {
            if (parent == null)
                return null;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == name)
                    return child;
            }

            return null;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null)
                return null;

            if (root.name == name)
                return root;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDeep(root.GetChild(i), name);
                if (match != null)
                    return match;
            }

            return null;
        }

        private static Transform FindDeepAny(Transform root, string primaryName, params string[] aliases)
        {
            Transform match = FindDeep(root, primaryName);
            if (match != null)
                return match;

            if (aliases == null)
                return null;

            for (int i = 0; i < aliases.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(aliases[i]))
                    continue;

                match = FindDeep(root, aliases[i]);
                if (match != null)
                    return match;
            }

            return null;
        }
    }
}
