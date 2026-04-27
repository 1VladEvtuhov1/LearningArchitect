using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.Effects
{
    public sealed class ChunkEffectsVariant : MonoBehaviour, IShowcaseStressTarget
    {
        public int count = 5000;
        public int visualCount = 240;
        public int visualLimit = 420;
        public float radius = 7f;
        public float speed = 0.8f;

        private Vector3[] positions;
        private Vector3[] velocities;
        private Transform[] visuals;

        public int ActiveCount
        {
            get { return count; }
        }

        private void Awake()
        {
            Rebuild(count);
        }

        public void SetStressLevel(int value)
        {
            if (value < 1)
                value = 1;

            if (count == value && positions != null && positions.Length == value)
                return;

            count = value;
            Rebuild(count);
        }

        private void Rebuild(int targetCount)
        {
            ClearVisuals();

            positions = new Vector3[count];
            velocities = new Vector3[count];

            int visible = Mathf.Min(visualCount, visualLimit);
            if (visible > count)
                visible = count;
            if (visible < 0)
                visible = 0;

            visuals = new Transform[visible];

            for (int i = 0; i < count; i++)
            {
                positions[i] = Random.insideUnitSphere * radius;
                positions[i].y = Mathf.Abs(positions[i].y) * 0.35f;
                velocities[i] = Random.onUnitSphere * speed;
                velocities[i].y *= 0.2f;
            }

            for (int i = 0; i < visible; i++)
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = "Chunk Effect Visual " + i;
                marker.transform.SetParent(transform, false);
                marker.transform.localPosition = positions[i];
                marker.transform.localScale = Vector3.one * 0.12f;
                visuals[i] = marker.transform;
            }
        }

        private void Update()
        {
            if (positions == null || velocities == null || visuals == null)
                return;

            float dt = Time.deltaTime;

            for (int i = 0; i < count; i++)
            {
                Vector3 position = positions[i] + velocities[i] * dt;

                if (position.sqrMagnitude > radius * radius)
                {
                    velocities[i] = -velocities[i];
                    position = positions[i] + velocities[i] * dt;
                }

                positions[i] = position;
            }

            for (int i = 0; i < visuals.Length; i++)
                visuals[i].localPosition = positions[i];
        }

        private void ClearVisuals()
        {
            if (visuals == null)
                return;

            for (int i = 0; i < visuals.Length; i++)
            {
                Transform visual = visuals[i];
                if (visual != null)
                    DestroyVisual(visual.gameObject);
            }
        }

        private static void DestroyVisual(GameObject visual)
        {
#if UNITY_EDITOR
            DestroyImmediate(visual);
#else
            Destroy(visual);
#endif
        }
    }
}
