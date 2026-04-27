using System;
using System.Text;
using UnityEngine;

namespace LearningArchitect.IndieEffects
{
    public sealed class IndieSimulationView : MonoBehaviour
    {
        [Header("Simulation")]
        public int entityCount = 32;
        public float startHealth = 100f;
        public float maxShield = 40f;
        public float baseSpeed = 4f;

        [Header("Demo Effects")]
        public float dotDamagePerSecond = 5f;
        public float healPerSecond = 2f;
        public float shieldPerSecond = 8f;
        public float slowMultiplier = 0.45f;
        public float effectDuration = 12f;

        [Header("Visuals")]
        public bool createVisuals = true;
        public int columns = 8;
        public float spacing = 1.4f;
        public Color healthyColor = new Color(0.15f, 0.8f, 0.25f, 1f);
        public Color lowHealthColor = new Color(0.95f, 0.2f, 0.08f, 1f);
        public Color shieldColor = new Color(0.2f, 0.55f, 1f, 1f);
        public Color slowColor = new Color(0.45f, 0.25f, 1f, 1f);

        [Header("Debug")]
        public bool showOverlay = true;

        [NonSerialized] public SimulationSystem Simulation;

        private GameObject _root;
        private Renderer[] _renderers;
        private Material _material;
        private MaterialPropertyBlock _block;
        private GUIStyle _style;
        private readonly StringBuilder _builder = new StringBuilder(256);
        private string _overlayText = string.Empty;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private void Start()
        {
            RestartDemo();
        }

        private void Update()
        {
            if (Simulation == null)
                return;

            Simulation.Tick(Time.deltaTime);
            UpdateVisuals();
            BuildOverlay();
        }

        private void OnDisable()
        {
            DestroyVisuals();
        }

        private void OnGUI()
        {
            if (!showOverlay)
                return;

            if (_style == null)
                CreateStyle();

            GUI.Label(new Rect(12f, 340f, 360f, 180f), _overlayText, _style);
        }

        [ContextMenu("Restart Demo")]
        public void RestartDemo()
        {
            Simulation = new SimulationSystem(entityCount, entityCount * 4);

            for (int i = 0; i < entityCount; i++)
            {
                Entity entity = Simulation.CreateEntity(startHealth, maxShield, baseSpeed);
                Simulation.AddDot(entity, dotDamagePerSecond + (i % 3), effectDuration);

                if ((i & 1) == 0)
                    Simulation.AddHeal(entity, healPerSecond, effectDuration);

                if ((i % 3) == 0)
                    Simulation.AddShield(entity, shieldPerSecond, effectDuration * 0.5f);

                if ((i % 4) == 0)
                    Simulation.AddSlow(entity, slowMultiplier, effectDuration);
            }

            if (createVisuals && Application.isPlaying)
                BuildVisuals();
            else
                DestroyVisuals();
        }

        private void BuildVisuals()
        {
            DestroyVisuals();

            _root = new GameObject("Indie Effect Entities");
            _renderers = new Renderer[entityCount];
            _block = new MaterialPropertyBlock();

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            _material = new Material(shader);
            _material.enableInstancing = true;

            int safeColumns = columns < 1 ? 1 : columns;
            float xOffset = (safeColumns - 1) * spacing * 0.5f;

            for (int i = 0; i < entityCount; i++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = "Indie Entity " + i;
                cube.transform.SetParent(_root.transform, false);
                cube.transform.localPosition = new Vector3((i % safeColumns) * spacing - xOffset, 0f, (i / safeColumns) * spacing);
                cube.transform.localScale = Vector3.one;

                Collider collider = cube.GetComponent<Collider>();
                if (collider != null)
                    Destroy(collider);

                Renderer renderer = cube.GetComponent<Renderer>();
                renderer.sharedMaterial = _material;
                _renderers[i] = renderer;
            }
        }

        private void UpdateVisuals()
        {
            if (_renderers == null || Simulation == null)
                return;

            for (int i = 0; i < _renderers.Length; i++)
            {
                Entity entity = new Entity(i);
                Health health = Simulation.GetHealth(entity);
                Shield shield = Simulation.GetShield(entity);
                Movement movement = Simulation.GetMovement(entity);

                float health01 = health.MaxValue <= 0f ? 0f : health.Value / health.MaxValue;
                health01 = Mathf.Clamp01(health01);

                Color color = Color.Lerp(lowHealthColor, healthyColor, health01);
                if (shield.Value > 0.01f)
                    color = Color.Lerp(color, shieldColor, Mathf.Clamp01(shield.Value / shield.MaxValue));
                if (movement.CurrentSpeed < movement.BaseSpeed)
                    color = Color.Lerp(color, slowColor, 0.35f);

                _block.SetColor(BaseColorId, color);
                _block.SetColor(ColorId, color);
                _renderers[i].SetPropertyBlock(_block);
            }
        }

        private void BuildOverlay()
        {
            if (!showOverlay || Simulation == null)
                return;

            IndieEffectMetrics metrics = Simulation.Metrics;
            _builder.Length = 0;
            _builder.Append("Indie Effects\n");
            _builder.Append("Entities: ");
            _builder.Append(metrics.entities);
            _builder.Append('\n');
            _builder.Append("DoT / Heal / Shield / Slow: ");
            _builder.Append(metrics.dots);
            _builder.Append(" / ");
            _builder.Append(metrics.heals);
            _builder.Append(" / ");
            _builder.Append(metrics.shields);
            _builder.Append(" / ");
            _builder.Append(metrics.slows);
            _builder.Append('\n');
            _builder.Append("Processed: ");
            _builder.Append(metrics.processedEffects);
            _builder.Append('\n');
            _builder.Append("Applied entities: ");
            _builder.Append(metrics.appliedEntities);
            _builder.Append('\n');
            _builder.Append("Damage / Heal / Shield: ");
            _builder.Append(metrics.damageApplied.ToString("0.0"));
            _builder.Append(" / ");
            _builder.Append(metrics.healingApplied.ToString("0.0"));
            _builder.Append(" / ");
            _builder.Append(metrics.shieldApplied.ToString("0.0"));
            _overlayText = _builder.ToString();
        }

        private void CreateStyle()
        {
            _style = new GUIStyle(GUI.skin.box);
            _style.alignment = TextAnchor.UpperLeft;
            _style.fontSize = 15;
            _style.normal.textColor = Color.white;
            _style.padding = new RectOffset(10, 10, 8, 8);
        }

        private void DestroyVisuals()
        {
            if (_root == null)
                return;

            if (Application.isPlaying)
                Destroy(_root);
            else
                DestroyImmediate(_root);

            _root = null;
            _renderers = null;
        }
    }
}
