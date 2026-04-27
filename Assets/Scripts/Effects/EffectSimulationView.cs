using System;
using System.Text;
using UnityEngine;

namespace LearningArchitect.Effects
{
    public enum EffectDemoScenario
    {
        Entities1000FiveEffects,
        Effects100000,
        Hotspot100000
    }

    public sealed class EffectSimulationView : MonoBehaviour
    {
        [Header("Simulation")]
        public EffectDemoScenario scenario = EffectDemoScenario.Entities1000FiveEffects;
        public int entityCount = 1000;
        public int initialHealth = 1000;
        public int tickRate = 60;
        public int maxTicksPerFrame = 4;

        [Header("Scenario 1")]
        public int effectsPerEntity = 5;

        [Header("Stress Scenarios")]
        public int stressEffectCount = 100000;
        public int hotspotEffectCount = 100000;

        [Header("Visuals")]
        public bool createVisuals = true;
        public int visualEntityLimit = 256;
        public int visualColumns = 32;
        public float visualSpacing = 1.2f;
        public float visualRefreshSeconds = 0.1f;
        public bool showDamagePulse = true;
        public int pulseDamageUnit = 1;
        public float damagePulseDecayPerSecond = 3f;
        public Color fullHealthColor = new Color(0.15f, 0.9f, 0.25f, 1f);
        public Color emptyHealthColor = new Color(0.95f, 0.15f, 0.08f, 1f);
        public Color damagePulseColor = new Color(1f, 0.92f, 0.15f, 1f);

        [Header("Debug")]
        public bool showOverlay = true;
        public float overlayRefreshSeconds = 0.25f;
        public int overlayChunkRows = 4;

        [NonSerialized] public EffectManager manager;

        private Renderer[] _visualRenderers;
        private int[] _visualEntities;
        private int[] _lastVisualHealth;
        private float[] _damageFlash;
        private Material _visualMaterial;
        private MaterialPropertyBlock _propertyBlock;
        private GameObject _visualRoot;
        private GUIStyle _overlayStyle;
        private readonly StringBuilder _overlayBuilder = new StringBuilder(512);
        private string _overlayText = string.Empty;
        private float _nextVisualRefresh;
        private float _nextOverlayRefresh;
        private float _smoothedFps;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private void Start()
        {
            RestartScenario();
        }

        private void Update()
        {
            if (manager == null)
                return;

            manager.Tick(Time.deltaTime);
            UpdateVisualsIfNeeded();
            UpdateOverlayIfNeeded();
        }

        private void OnDisable()
        {
            DestroyVisuals();
        }

        private void OnGUI()
        {
            if (!showOverlay)
                return;

            if (_overlayStyle == null)
                CreateOverlayStyle();

            GUI.Label(new Rect(12f, 12f, 460f, 320f), _overlayText, _overlayStyle);
        }

        [ContextMenu("Restart Scenario")]
        public void RestartScenario()
        {
            int totalEffects = GetScenarioEffectCount();
            int chunkCount = (entityCount + EffectManager.DefaultChunkSize - 1) / EffectManager.DefaultChunkSize;
            int effectCapacityPerChunk = CalculateEffectCapacityPerChunk(totalEffects, chunkCount);
            int commandCapacity = totalEffects + 1024;

            manager = new EffectManager(
                entityCount,
                effectCapacityPerChunk,
                commandCapacity,
                tickRate,
                EffectManager.DefaultChunkSize,
                maxTicksPerFrame);

            manager.InitializeHealth(initialHealth);
            QueueScenarioEffects(totalEffects);
            manager.FlushPendingCommands();

            if (createVisuals && Application.isPlaying)
                BuildVisuals();
            else
                DestroyVisuals();

            _nextVisualRefresh = 0f;
            _nextOverlayRefresh = 0f;
            _overlayText = string.Empty;
        }

        private int GetScenarioEffectCount()
        {
            if (scenario == EffectDemoScenario.Entities1000FiveEffects)
                return entityCount * effectsPerEntity;
            if (scenario == EffectDemoScenario.Hotspot100000)
                return hotspotEffectCount;

            return stressEffectCount;
        }

        private int CalculateEffectCapacityPerChunk(int totalEffects, int chunkCount)
        {
            if (scenario == EffectDemoScenario.Hotspot100000)
                return totalEffects + 64;

            int average = (totalEffects + chunkCount - 1) / chunkCount;
            int slack = (totalEffects / 10) + 1024;
            return average + slack;
        }

        private void QueueScenarioEffects(int totalEffects)
        {
            if (scenario == EffectDemoScenario.Entities1000FiveEffects)
            {
                for (int entity = 0; entity < entityCount; entity++)
                {
                    for (int effect = 0; effect < effectsPerEntity; effect++)
                    {
                        int interval = 20 + (effect * 10);
                        int duration = tickRate * 120;
                        int damage = 1 + ((entity + effect) % 4);
                        manager.TryQueueAddPeriodicDamage(entity, damage, interval, duration);
                    }
                }

                return;
            }

            bool hotspot = scenario == EffectDemoScenario.Hotspot100000;
            for (int i = 0; i < totalEffects; i++)
            {
                uint hash = Hash((uint)i + 0x9E3779B9u);
                int target = hotspot ? 0 : (int)(hash % (uint)entityCount);
                int interval = 15 + (int)((hash >> 8) % 90u);
                int duration = tickRate * (60 + (int)((hash >> 16) % 120u));
                int damage = 1 + (int)((hash >> 24) % 4u);

                manager.TryQueueAddPeriodicDamage(target, damage, interval, duration);
            }
        }

        private void BuildVisuals()
        {
            DestroyVisuals();

            int count = visualEntityLimit;
            if (count > entityCount)
                count = entityCount;
            if (count < 0)
                count = 0;

            _visualRenderers = new Renderer[count];
            _visualEntities = new int[count];
            _lastVisualHealth = new int[count];
            _damageFlash = new float[count];
            _propertyBlock = new MaterialPropertyBlock();
            _visualRoot = new GameObject("Effect Simulation Entities");

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            _visualMaterial = new Material(shader);
            _visualMaterial.enableInstancing = true;

            int columns = visualColumns < 1 ? 1 : visualColumns;
            float xOffset = (columns - 1) * visualSpacing * 0.5f;

            for (int i = 0; i < count; i++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = "Effect Entity " + i;
                cube.transform.SetParent(_visualRoot.transform, false);

                int x = i % columns;
                int z = i / columns;
                cube.transform.localPosition = new Vector3((x * visualSpacing) - xOffset, 0f, z * visualSpacing);
                cube.transform.localScale = Vector3.one * 0.85f;

                Collider collider = cube.GetComponent<Collider>();
                if (collider != null)
                    Destroy(collider);

                Renderer renderer = cube.GetComponent<Renderer>();
                renderer.sharedMaterial = _visualMaterial;

                _visualRenderers[i] = renderer;
                _visualEntities[i] = i;
                _lastVisualHealth[i] = manager.GetHealth(i);
            }
        }

        private void DestroyVisuals()
        {
            if (_visualRoot == null)
                return;

            if (Application.isPlaying)
                Destroy(_visualRoot);
            else
                DestroyImmediate(_visualRoot);

            _visualRoot = null;
            _visualRenderers = null;
            _visualEntities = null;
            _lastVisualHealth = null;
            _damageFlash = null;
        }

        private void UpdateVisualsIfNeeded()
        {
            if (!createVisuals || _visualRenderers == null || Time.unscaledTime < _nextVisualRefresh)
                return;

            _nextVisualRefresh = Time.unscaledTime + visualRefreshSeconds;

            for (int i = 0; i < _visualRenderers.Length; i++)
            {
                int entity = _visualEntities[i];
                int health = manager.GetHealth(entity);
                int maxHealth = manager.GetMaxHealth(entity);
                float health01 = maxHealth <= 0 ? 0f : (float)health / maxHealth;
                if (health01 < 0f)
                    health01 = 0f;

                Color color = Color.Lerp(emptyHealthColor, fullHealthColor, health01);
                int damageSinceLastVisual = _lastVisualHealth[i] - health;
                _lastVisualHealth[i] = health;

                if (showDamagePulse && damageSinceLastVisual > 0)
                {
                    int unit = pulseDamageUnit < 1 ? 1 : pulseDamageUnit;
                    float pulse = Mathf.Clamp01((float)damageSinceLastVisual / unit);
                    if (pulse > _damageFlash[i])
                        _damageFlash[i] = pulse;
                }

                if (showDamagePulse && _damageFlash[i] > 0f)
                {
                    color = Color.Lerp(color, damagePulseColor, _damageFlash[i]);
                    _damageFlash[i] -= visualRefreshSeconds * damagePulseDecayPerSecond;
                    if (_damageFlash[i] < 0f)
                        _damageFlash[i] = 0f;
                }

                _propertyBlock.SetColor(BaseColorId, color);
                _propertyBlock.SetColor(ColorId, color);
                _visualRenderers[i].SetPropertyBlock(_propertyBlock);
            }
        }

        private void UpdateOverlayIfNeeded()
        {
            if (!showOverlay || Time.unscaledTime < _nextOverlayRefresh)
                return;

            _nextOverlayRefresh = Time.unscaledTime + overlayRefreshSeconds;

            float instantFps = Time.unscaledDeltaTime <= 0f ? 0f : 1f / Time.unscaledDeltaTime;
            _smoothedFps = _smoothedFps <= 0f ? instantFps : Mathf.Lerp(_smoothedFps, instantFps, 0.15f);

            EffectMetrics metrics = manager.Metrics;
            _overlayBuilder.Length = 0;
            _overlayBuilder.Append("FPS: ");
            _overlayBuilder.Append(Mathf.RoundToInt(_smoothedFps));
            _overlayBuilder.Append('\n');
            _overlayBuilder.Append("Scenario: ");
            _overlayBuilder.Append(scenario);
            _overlayBuilder.Append('\n');
            _overlayBuilder.Append("Entities: ");
            _overlayBuilder.Append(entityCount);
            _overlayBuilder.Append('\n');
            _overlayBuilder.Append("Chunks: ");
            _overlayBuilder.Append(manager.ChunkCount);
            _overlayBuilder.Append('\n');
            _overlayBuilder.Append("Effects: ");
            _overlayBuilder.Append(metrics.totalEffects);
            _overlayBuilder.Append('\n');
            _overlayBuilder.Append("Scheduled: ");
            _overlayBuilder.Append(metrics.scheduledEffects);
            _overlayBuilder.Append('\n');
            _overlayBuilder.Append("Processed: ");
            _overlayBuilder.Append(metrics.processedEffects);
            _overlayBuilder.Append('\n');
            _overlayBuilder.Append("Applied entities: ");
            _overlayBuilder.Append(metrics.appliedEntities);
            _overlayBuilder.Append('\n');
            _overlayBuilder.Append("Damage/frame: ");
            _overlayBuilder.Append(metrics.appliedDamage);
            _overlayBuilder.Append('\n');
            _overlayBuilder.Append("Dirty entities: ");
            _overlayBuilder.Append(metrics.dirtyEntities);
            _overlayBuilder.Append('\n');
            _overlayBuilder.Append("Sim ticks: ");
            _overlayBuilder.Append(metrics.simulatedTicks);
            _overlayBuilder.Append('\n');
            _overlayBuilder.Append("Tick: ");
            _overlayBuilder.Append(metrics.tickTimeMs.ToString("0.000"));
            _overlayBuilder.Append(" ms\n");
            _overlayBuilder.Append("Dropped commands: ");
            _overlayBuilder.Append(metrics.droppedCommands);
            AppendChunkDebug();
            _overlayText = _overlayBuilder.ToString();
        }

        private void AppendChunkDebug()
        {
            if (manager == null || overlayChunkRows <= 0)
                return;

            Chunk[] chunks = manager.Chunks;
            int count = overlayChunkRows;
            if (count > chunks.Length)
                count = chunks.Length;

            _overlayBuilder.Append("\n\nChunks");
            for (int i = 0; i < count; i++)
            {
                Chunk chunk = chunks[i];
                _overlayBuilder.Append("\nC");
                _overlayBuilder.Append(i);
                _overlayBuilder.Append(" e:");
                _overlayBuilder.Append(chunk.Effects.Count);
                _overlayBuilder.Append(" s:");
                _overlayBuilder.Append(chunk.lastScheduledEffects);
                _overlayBuilder.Append(" p:");
                _overlayBuilder.Append(chunk.lastProcessedEffects);
                _overlayBuilder.Append(" a:");
                _overlayBuilder.Append(chunk.lastAppliedEntities);
                _overlayBuilder.Append(" d:");
                _overlayBuilder.Append(chunk.lastAppliedDamage);
            }
        }

        private void CreateOverlayStyle()
        {
            _overlayStyle = new GUIStyle(GUI.skin.box);
            _overlayStyle.alignment = TextAnchor.UpperLeft;
            _overlayStyle.fontSize = 16;
            _overlayStyle.normal.textColor = Color.white;
            _overlayStyle.padding = new RectOffset(12, 12, 10, 10);
        }

        private static uint Hash(uint value)
        {
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return value;
        }
    }
}
