using System.Collections.Generic;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.Effects
{
    public sealed class IndieEffectsVariant : MonoBehaviour, IShowcaseStressTarget
    {
        public GameObject effectPrefab;
        public int count = 500;
        public float radius = 6f;
        public float rotateSpeed = 40f;

        private readonly List<GameObject> effects = new List<GameObject>(512);

        public int ActiveCount
        {
            get { return effects.Count; }
        }

        private void Awake()
        {
            Rebuild(count);
        }

        public void SetStressLevel(int value)
        {
            if (value < 1)
                value = 1;

            if (count == value && effects.Count == value)
                return;

            count = value;
            Rebuild(count);
        }

        private void Rebuild(int targetCount)
        {
            ClearEffects();

            if (effects.Capacity < targetCount)
                effects.Capacity = targetCount;

            for (int i = 0; i < count; i++)
            {
                Vector3 position = Random.insideUnitSphere * radius;
                position.y = Mathf.Abs(position.y) * 0.3f;

                GameObject instance = effectPrefab == null
                    ? GameObject.CreatePrimitive(PrimitiveType.Sphere)
                    : Instantiate(effectPrefab);

                instance.name = "Indie Effect " + i;
                instance.transform.SetParent(transform, false);
                instance.transform.localPosition = position;
                instance.transform.localScale = Vector3.one * 0.14f;
                effects.Add(instance);
            }
        }

        private void Update()
        {
            float angle = rotateSpeed * Time.deltaTime;

            for (int i = 0; i < effects.Count; i++)
            {
                GameObject effect = effects[i];
                if (effect != null)
                    effect.transform.Rotate(Vector3.up, angle, Space.World);
            }
        }

        private void ClearEffects()
        {
            for (int i = 0; i < effects.Count; i++)
            {
                GameObject effect = effects[i];
                if (effect != null)
                    DestroyEffect(effect);
            }

            effects.Clear();
        }

        private static void DestroyEffect(GameObject effect)
        {
#if UNITY_EDITOR
            DestroyImmediate(effect);
#else
            Destroy(effect);
#endif
        }
    }
}
