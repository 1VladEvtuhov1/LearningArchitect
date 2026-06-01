using UnityEngine;

namespace LearningArchitect.Core
{
    public static class ShowcaseVisualInstanceFactory
    {
        public static GameObject CreateMarker(
            Transform parent,
            string name,
            GameObject visualPrefab,
            Vector3 localScale)
        {
            if (visualPrefab == null)
                throw new MissingReferenceException($"Visual prefab is required for runtime marker '{name}'.");

            GameObject marker = Object.Instantiate(visualPrefab, parent, false);

            marker.name = name;
            marker.transform.localScale = localScale;
            RemoveColliders(marker);
            return marker;
        }

        public static Renderer FindPrimaryRenderer(GameObject root)
        {
            if (root == null)
                return null;

            Renderer directRenderer = root.GetComponent<Renderer>();
            if (directRenderer != null)
                return directRenderer;

            return root.GetComponentInChildren<Renderer>(true);
        }

        private static void RemoveColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (Application.isPlaying)
                    Object.Destroy(colliders[i]);
                else
                    Object.DestroyImmediate(colliders[i]);
            }
        }
    }
}
