using UnityEngine;

namespace LearningArchitect.Core
{
    internal static class ShowcasePrimitiveMaterialUtility
    {
        private static readonly Color DefaultBaseColor = new Color(0.48f, 0.51f, 0.58f, 1f);
        private static Material sharedMaterial;

        public static void Apply(GameObject target)
        {
            if (target == null)
                return;

            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer == null)
                return;

            renderer.sharedMaterial = GetOrCreateSharedMaterial();
        }

        private static Material GetOrCreateSharedMaterial()
        {
            if (sharedMaterial != null)
                return sharedMaterial;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                ?? Shader.Find("Standard");

            if (shader == null)
                return null;

            sharedMaterial = new Material(shader)
            {
                name = "ShowcaseRuntimePrimitiveMaterial",
                hideFlags = HideFlags.HideAndDontSave
            };

            if (sharedMaterial.HasProperty("_Surface"))
                sharedMaterial.SetFloat("_Surface", 0f);

            if (sharedMaterial.HasProperty("_WorkflowMode"))
                sharedMaterial.SetFloat("_WorkflowMode", 1f);

            if (sharedMaterial.HasProperty("_BaseColor"))
                sharedMaterial.SetColor("_BaseColor", DefaultBaseColor);

            if (sharedMaterial.HasProperty("_Color"))
                sharedMaterial.SetColor("_Color", DefaultBaseColor);

            if (sharedMaterial.HasProperty("_Smoothness"))
                sharedMaterial.SetFloat("_Smoothness", 0.15f);

            if (sharedMaterial.HasProperty("_Glossiness"))
                sharedMaterial.SetFloat("_Glossiness", 0.15f);

            return sharedMaterial;
        }
    }
}
