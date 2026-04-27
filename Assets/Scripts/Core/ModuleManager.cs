using LearningArchitect.UI;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LearningArchitect.Core
{
    public sealed class ModuleManager : MonoBehaviour
    {
        [Header("Modules")]
        public ModuleDefinitionSO[] modules;
        public Transform moduleRoot;

        [Header("UI")]
        public HubUI hubUI;
        public DescriptionPanel descriptionPanel;
        public CanvasGroup transitionOverlay;

        [Header("Presentation")]
        public float transitionDuration = 0.18f;

        [Header("Input")]
        public KeyCode previousModuleKey = KeyCode.LeftArrow;
        public KeyCode nextModuleKey = KeyCode.RightArrow;
        public KeyCode previousVariantKey = KeyCode.UpArrow;
        public KeyCode nextVariantKey = KeyCode.DownArrow;

        private int currentModuleIndex;
        private int currentVariantIndex;
        private GameObject activeInstance;
        private IModule activeModule;
        private IShowcaseStressTarget activeStressTarget;
        private int currentStressLevel = 1000;
        private int transitionState;
        private float transitionTimer;
        private float transitionAlphaFrom;

        public int CurrentModuleIndex
        {
            get { return currentModuleIndex; }
        }

        public int CurrentVariantIndex
        {
            get { return currentVariantIndex; }
        }

        public ModuleDefinitionSO CurrentModule
        {
            get
            {
                if (modules == null || modules.Length == 0)
                    return null;

                return modules[currentModuleIndex];
            }
        }

        public VariantDefinitionSO CurrentVariant
        {
            get
            {
                ModuleDefinitionSO module = CurrentModule;
                if (module == null || module.variants == null || module.variants.Length == 0)
                    return null;

                return module.variants[currentVariantIndex];
            }
        }

        public int CurrentStressLevel
        {
            get { return currentStressLevel; }
        }

        public int ActiveItemCount
        {
            get { return activeStressTarget == null ? 0 : activeStressTarget.ActiveCount; }
        }

        private void Start()
        {
            if (moduleRoot == null)
                moduleRoot = transform;

            if (transitionOverlay != null)
            {
                transitionOverlay.alpha = 1f;
                transitionOverlay.blocksRaycasts = false;
                transitionOverlay.interactable = false;
            }

            LoadCurrentImmediate();

            if (transitionOverlay != null)
                BeginFadeIn(1f);
        }

        private void Update()
        {
            if (WasPressed(previousModuleKey))
            {
                if (hubUI != null)
                    hubUI.PlayModuleSwitchFeedback(-1);

                PreviousModule();
            }

            if (WasPressed(nextModuleKey))
            {
                if (hubUI != null)
                    hubUI.PlayModuleSwitchFeedback(1);

                NextModule();
            }

            if (WasPressed(previousVariantKey))
            {
                if (hubUI != null)
                    hubUI.PlayVariantSwitchFeedback(-1);

                PreviousVariant();
            }

            if (WasPressed(nextVariantKey))
            {
                if (hubUI != null)
                    hubUI.PlayVariantSwitchFeedback(1);

                NextVariant();
            }

            UpdateTransition();
        }

        private void OnDestroy()
        {
            UnloadCurrent();
        }

        public void NextModule()
        {
            if (!HasModules())
                return;

            currentModuleIndex = (currentModuleIndex + 1) % modules.Length;
            currentVariantIndex = 0;
            LoadCurrent();
        }

        public void PreviousModule()
        {
            if (!HasModules())
                return;

            currentModuleIndex = (currentModuleIndex + modules.Length - 1) % modules.Length;
            currentVariantIndex = 0;
            LoadCurrent();
        }

        public void NextVariant()
        {
            ModuleDefinitionSO module = CurrentModule;
            if (!HasVariants(module))
                return;

            currentVariantIndex = (currentVariantIndex + 1) % module.variants.Length;
            LoadCurrent();
        }

        public void PreviousVariant()
        {
            ModuleDefinitionSO module = CurrentModule;
            if (!HasVariants(module))
                return;

            currentVariantIndex = (currentVariantIndex + module.variants.Length - 1) % module.variants.Length;
            LoadCurrent();
        }

        public void SetStressLevel(int count)
        {
            if (count < 1)
                count = 1;

            currentStressLevel = count;

            if (activeStressTarget != null)
                activeStressTarget.SetStressLevel(currentStressLevel);
        }

        public void LoadCurrent()
        {
            if (!isActiveAndEnabled || transitionOverlay == null || transitionDuration <= 0f)
            {
                LoadCurrentImmediate();
                return;
            }

            UpdateUI(CurrentModule, CurrentVariant);

            if (Application.isPlaying && Time.frameCount > 1)
            {
                transitionState = 1;
                transitionTimer = 0f;
                transitionAlphaFrom = transitionOverlay.alpha;
                return;
            }

            transitionOverlay.alpha = 1f;
            LoadCurrentImmediate();
            BeginFadeIn(1f);
        }

        private void UpdateTransition()
        {
            if (transitionOverlay == null || transitionState == 0)
                return;

            float duration = Mathf.Max(0.01f, transitionDuration);
            transitionTimer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(transitionTimer / duration);
            t = t * t * (3f - 2f * t);

            if (transitionState == 1)
            {
                transitionOverlay.alpha = Mathf.Lerp(transitionAlphaFrom, 1f, t);

                if (transitionTimer >= duration)
                {
                    LoadCurrentImmediate();
                    BeginFadeIn(1f);
                }

                return;
            }

            transitionOverlay.alpha = Mathf.Lerp(transitionAlphaFrom, 0f, t);

            if (transitionTimer >= duration)
            {
                transitionOverlay.alpha = 0f;
                transitionState = 0;
            }
        }

        private void BeginFadeIn(float from)
        {
            transitionState = 2;
            transitionTimer = 0f;
            transitionAlphaFrom = from;

            if (transitionOverlay != null)
                transitionOverlay.alpha = from;
        }

        private void LoadCurrentImmediate()
        {
            UnloadCurrent();

            ModuleDefinitionSO module = CurrentModule;
            VariantDefinitionSO variant = CurrentVariant;

            if (variant != null && variant.prefab != null)
            {
                activeInstance = Instantiate(variant.prefab, moduleRoot);
                activeInstance.name = variant.prefab.name;
                activeModule = activeInstance.GetComponent<IModule>();
                activeStressTarget = activeInstance.GetComponent<IShowcaseStressTarget>();

                if (activeModule != null)
                    activeModule.Enter();

                if (activeStressTarget != null)
                    activeStressTarget.SetStressLevel(currentStressLevel);
            }

            UpdateUI(module, variant);
        }

        private void UnloadCurrent()
        {
            if (activeModule != null)
            {
                activeModule.Exit();
                activeModule = null;
            }

            activeStressTarget = null;

            if (activeInstance != null)
            {
                DestroyInstance(activeInstance);
                activeInstance = null;
            }
        }

        private void UpdateUI(ModuleDefinitionSO module, VariantDefinitionSO variant)
        {
            if (hubUI != null)
                hubUI.SetModule(module, variant);

            if (descriptionPanel != null)
                descriptionPanel.SetContent(module, variant);
        }

        private bool HasModules()
        {
            return modules != null && modules.Length > 0;
        }

        private static bool HasVariants(ModuleDefinitionSO module)
        {
            return module != null && module.variants != null && module.variants.Length > 0;
        }

        private static bool WasPressed(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                switch (key)
                {
                    case KeyCode.LeftArrow:
                        return keyboard.leftArrowKey.wasPressedThisFrame;
                    case KeyCode.RightArrow:
                        return keyboard.rightArrowKey.wasPressedThisFrame;
                    case KeyCode.UpArrow:
                        return keyboard.upArrowKey.wasPressedThisFrame;
                    case KeyCode.DownArrow:
                        return keyboard.downArrowKey.wasPressedThisFrame;
                }
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(key);
#else
            return false;
#endif
        }

        private static void DestroyInstance(GameObject instance)
        {
            if (instance == null)
                return;

#if UNITY_EDITOR
            DestroyImmediate(instance);
#else
            Destroy(instance);
#endif
        }
    }
}
