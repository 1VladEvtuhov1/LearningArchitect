using System;
using UnityEngine;

namespace LearningArchitect.Modules.AI
{
    public sealed class AiWorld
    {
        public int Count;

        public Vector3[] Positions;
        public Vector3[] Targets;
        public Vector3[] HomePositions;

        public float[] Energy;
        public float[] Timers;

        public int[] State;
        public int[] Task;
        public uint[] RandomStates;

        public AiWorld(int count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count));

            Count = count;
            Positions = new Vector3[count];
            Targets = new Vector3[count];
            HomePositions = new Vector3[count];
            Energy = new float[count];
            Timers = new float[count];
            State = new int[count];
            Task = new int[count];
            RandomStates = new uint[count];
        }
    }
}
