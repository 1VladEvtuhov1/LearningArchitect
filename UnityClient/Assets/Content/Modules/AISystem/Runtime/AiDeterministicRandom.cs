using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.AI
{
    internal static class AiDeterministicRandom
    {
        private const float InverseMax24Bit = 1f / 16777216f;

        public static uint CreateState(int index, uint salt)
        {
            uint state = (uint)index + 1u;
            state ^= salt + 0x9E3779B9u;
            state ^= state >> 16;
            state *= 0x7FEB352Du;
            state ^= state >> 15;
            state *= 0x846CA68Bu;
            state ^= state >> 16;
            return state == 0u ? 1u : state;
        }

        public static float NextFloat01(ref uint state)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;

            if (state == 0u)
                state = 1u;

            return (state & 0x00FFFFFFu) * InverseMax24Bit;
        }

        public static Vector3 NextPointOnDisc(ref uint state, float radius)
        {
            float angle = NextFloat01(ref state) * Mathf.PI * 2f;
            float distance = Mathf.Sqrt(NextFloat01(ref state)) * radius;
            return new Vector3(
                Mathf.Cos(angle) * distance,
                ShowcaseSpawnLayout.SurfaceY,
                Mathf.Sin(angle) * distance);
        }
    }
}
