using UnityEngine;

namespace LearningArchitect.Core
{
    /// <summary>
    /// Stress presets and spawn counts are the same value: how many scene objects a variant creates.
    /// </summary>
    public static class ShowcaseStressSpawn
    {
        public static readonly int[] DefaultPresets = { 80, 160, 240 };

        public static int Clamp(int requested, int visualLimit)
        {
            int limit = Mathf.Max(1, visualLimit);
            return Mathf.Clamp(requested, 1, limit);
        }
    }
}
