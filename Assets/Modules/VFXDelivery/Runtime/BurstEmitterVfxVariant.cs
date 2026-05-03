using System;
using System.Diagnostics;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.VFX
{
    public sealed class BurstEmitterVfxVariant : MonoBehaviour, IShowcaseStressTarget, IShowcaseMetricsSource
    {
        private struct EmitterRig
        {
            public Transform Root;
            public ParticleSystem ParticleSystem;
            public float Angle;
            public float OrbitRadius;
            public float Height;
            public float NextBurstTime;
            public float OrbitRate;
        }

        [SerializeField] private int count = 120;
        [SerializeField] private int visualLimit = 120;
        [SerializeField] private float orbitRadius = 6.4f;
        [SerializeField] private float baseBurstInterval = 0.28f;
        [SerializeField] private Material particleMaterial;

        private EmitterRig[] emitters;
        private float moduleCpuMs;

        public int ActiveCount => emitters == null ? 0 : emitters.Length;

        private void Awake()
        {
            Rebuild(count);
        }

        private void Update()
        {
            if (emitters == null)
                return;

            long startedAt = Stopwatch.GetTimestamp();
            float time = Time.time;
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < emitters.Length; i++)
            {
                EmitterRig emitter = emitters[i];
                emitter.Angle += deltaTime * emitter.OrbitRate;

                float radius = emitter.OrbitRadius;
                emitter.Root.localPosition = new Vector3(
                    Mathf.Cos(emitter.Angle) * radius,
                    emitter.Height,
                    Mathf.Sin(emitter.Angle) * radius);

                emitter.Root.localRotation = Quaternion.Euler(-90f, emitter.Angle * Mathf.Rad2Deg, 0f);

                if (time >= emitter.NextBurstTime)
                {
                    emitter.ParticleSystem.Emit(4 + (i % 3));
                    emitter.NextBurstTime = time + baseBurstInterval + ((i % 5) * 0.03f);
                }

                emitters[i] = emitter;
            }

            moduleCpuMs = (float)((Stopwatch.GetTimestamp() - startedAt) * 1000d / Stopwatch.Frequency);
        }

        public ShowcaseMetricsSnapshot GetMetricsSnapshot()
        {
            return new ShowcaseMetricsSnapshot(count, ActiveCount, moduleCpuMs);
        }

        public void SetStressLevel(int value)
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (count == value)
                return;

            count = value;
            Rebuild(count);
        }

        private void Rebuild(int targetCount)
        {
            ClearEmitters();

            int visible = Mathf.Min(targetCount, visualLimit);
            emitters = new EmitterRig[visible];
            for (int i = 0; i < visible; i++)
                emitters[i] = CreateEmitter(i, visible);
        }

        private EmitterRig CreateEmitter(int index, int visibleCount)
        {
            GameObject root = new("VFX Emitter " + index);
            root.transform.SetParent(transform, false);

            ParticleSystem particleSystem = root.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particleSystem.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.85f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            main.maxParticles = 32;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startColor = Color.Lerp(new Color(0.22f, 0.88f, 1f), new Color(1f, 0.48f, 0.76f), index / (float)Mathf.Max(1, visibleCount - 1));

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 18f;
            shape.radius = 0.08f;

            ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = ResolveParticleMaterial();

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(main.startColor.color, 0f),
                    new GradientColorKey(Color.white, 0.5f),
                    new GradientColorKey(main.startColor.color * 0.7f, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.9f, 0.15f),
                    new GradientAlphaKey(0.4f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;

            return new EmitterRig
            {
                Root = root.transform,
                ParticleSystem = particleSystem,
                Angle = (Mathf.PI * 2f * index) / Mathf.Max(1, visibleCount),
                OrbitRadius = orbitRadius * (0.55f + ((index % 7) * 0.07f)),
                Height = 0.18f + ((index % 5) * 0.14f),
                NextBurstTime = Time.time + (index * 0.015f),
                OrbitRate = 0.55f + ((index % 6) * 0.09f)
            };
        }

        private Material ResolveParticleMaterial()
        {
            if (particleMaterial != null)
                return particleMaterial;

            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");

            if (shader == null)
                throw new InvalidOperationException("BurstEmitterVfxVariant could not find a compatible particle shader.");

            particleMaterial = new Material(shader)
            {
                name = "BurstEmitter Runtime Particle Material"
            };

            if (particleMaterial.HasProperty("_Surface"))
                particleMaterial.SetFloat("_Surface", 1f);
            if (particleMaterial.HasProperty("_Blend"))
                particleMaterial.SetFloat("_Blend", 0f);
            if (particleMaterial.HasProperty("_SrcBlend"))
                particleMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (particleMaterial.HasProperty("_DstBlend"))
                particleMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            if (particleMaterial.HasProperty("_ZWrite"))
                particleMaterial.SetFloat("_ZWrite", 0f);
            if (particleMaterial.HasProperty("_Cull"))
                particleMaterial.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
            if (particleMaterial.HasProperty("_BaseColor"))
                particleMaterial.SetColor("_BaseColor", Color.white);
            if (particleMaterial.HasProperty("_Color"))
                particleMaterial.SetColor("_Color", Color.white);
            if (particleMaterial.HasProperty("_BaseMap"))
                particleMaterial.SetTexture("_BaseMap", Texture2D.whiteTexture);
            if (particleMaterial.HasProperty("_MainTex"))
                particleMaterial.SetTexture("_MainTex", Texture2D.whiteTexture);

            return particleMaterial;
        }

        private void ClearEmitters()
        {
            if (emitters == null)
                return;

            for (int i = 0; i < emitters.Length; i++)
            {
                if (emitters[i].Root != null)
                    DestroyVisual(emitters[i].Root.gameObject);
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
