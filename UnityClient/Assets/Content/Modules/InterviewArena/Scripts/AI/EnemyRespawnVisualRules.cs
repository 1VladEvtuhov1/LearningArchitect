using System.Collections.Generic;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Pure helpers for which renderers enemy death/respawn should toggle.
    /// </summary>
    public static class EnemyRespawnVisualRules
    {
        public static bool IsPhysicsProxyRenderer(Renderer renderer, Transform root)
        {
            if (renderer == null || root == null || renderer.transform != root)
                return false;

            if (!(renderer is MeshRenderer))
                return false;

            return root.GetComponent<CapsuleCollider>() != null;
        }

        public static Renderer[] CollectManagedRenderers(Transform root)
        {
            if (root == null)
                return System.Array.Empty<Renderer>();

            Renderer[] all = root.GetComponentsInChildren<Renderer>(true);
            var managed = new List<Renderer>(all.Length);
            for (int i = 0; i < all.Length; i++)
            {
                Renderer renderer = all[i];
                if (renderer == null || IsPhysicsProxyRenderer(renderer, root))
                    continue;

                managed.Add(renderer);
            }

            return managed.ToArray();
        }
    }
}
