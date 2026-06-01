using System;
using LearningArchitect.Core;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LearningArchitect.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ShowcaseCommandRouter))]
    [RequireComponent(typeof(HubUI))]
    public sealed class ShowcaseInputController : MonoBehaviour
    {
        [SerializeField] private ShowcaseCommandRouter commands;
        [SerializeField] private HubUI hubUI;

        [Header("Input")]
        [SerializeField] private KeyCode previousModuleKey = KeyCode.LeftArrow;
        [SerializeField] private KeyCode nextModuleKey = KeyCode.RightArrow;
        [SerializeField] private KeyCode previousVariantKey = KeyCode.UpArrow;
        [SerializeField] private KeyCode nextVariantKey = KeyCode.DownArrow;
        [SerializeField] private KeyCode toggleDemoKey = KeyCode.G;

        private RecruiterDemoController recruiterDemo;

        private void Awake()
        {
            commands = commands != null ? commands : GetComponent<ShowcaseCommandRouter>();
            hubUI = hubUI != null ? hubUI : GetComponent<HubUI>();
            recruiterDemo = GetComponent<RecruiterDemoController>();

            if (commands == null)
                throw new InvalidOperationException($"{nameof(ShowcaseCommandRouter)} is required.");

            if (hubUI == null)
                throw new InvalidOperationException($"{nameof(HubUI)} is required.");
        }

        private void Update()
        {
            if (WasPressed(previousModuleKey))
            {
                hubUI.PlayModuleSwitchFeedback(-1);
                commands.PreviousModule();
            }

            if (WasPressed(nextModuleKey))
            {
                hubUI.PlayModuleSwitchFeedback(1);
                commands.NextModule();
            }

            if (WasPressed(previousVariantKey))
            {
                hubUI.PlayVariantSwitchFeedback(-1);
                commands.PreviousVariant();
            }

            if (WasPressed(nextVariantKey))
            {
                hubUI.PlayVariantSwitchFeedback(1);
                commands.NextVariant();
            }

            if (recruiterDemo != null && WasPressed(toggleDemoKey))
                recruiterDemo.ToggleDemoExternal();
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
    }
}
