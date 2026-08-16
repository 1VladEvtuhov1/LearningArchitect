using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LearningArchitect.Modules.InterviewArena
{
    public struct PlayerInputFrame
    {
        public Vector2 Move;
    }

    /// <summary>
    /// Samples input in Update; jump/dash/combat actions use timed buffers for FixedUpdate / LateUpdate consumption.
    /// Uses the Input System package only (no legacy UnityEngine.Input calls).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private float moveDeadZone = 0.12f;
        [SerializeField] private float actionInputBufferDuration = 0.2f;

        private PlayerInputFrame currentFrame;
        private TimedInputBuffer jumpBuffer;
        private TimedInputBuffer dashBuffer;
        private TimedInputBuffer meleeBuffer;
        private TimedInputBuffer crossbowBuffer;
        private bool stanceToggleBuffered;

        public PlayerInputFrame CurrentFrame => currentFrame;
        public bool JumpHeld { get; private set; }
        public bool InteractHeld { get; private set; }
        public bool BlockHeld { get; private set; }

        public void ApplyConfig(PlayerConfig config)
        {
            if (config == null)
                return;

            actionInputBufferDuration = config.ActionInputBufferDuration;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            jumpBuffer.Tick(deltaTime);
            dashBuffer.Tick(deltaTime);
            meleeBuffer.Tick(deltaTime);
            crossbowBuffer.Tick(deltaTime);

            currentFrame = ReadMoveFrame();
            JumpHeld = ReadJumpHeld();
            InteractHeld = ReadInteractHeld();
            BlockHeld = ReadBlockHeld();
            BufferActions();
        }

        public bool ConsumeJump() => jumpBuffer.Consume();

        public bool ConsumeDash() => dashBuffer.Consume();

        public bool ConsumeMeleeAttack() => meleeBuffer.Consume();

        public bool ConsumeCrossbowAttack() => crossbowBuffer.Consume();

        public bool ConsumeStanceToggle()
        {
            if (!stanceToggleBuffered)
                return false;

            stanceToggleBuffered = false;
            return true;
        }

        private void BufferActions()
        {
            if (WasJumpPressed())
                jumpBuffer.Press(actionInputBufferDuration);

            if (WasDashPressed())
                dashBuffer.Press(actionInputBufferDuration);

            if (WasMeleePressed())
                meleeBuffer.Press(actionInputBufferDuration);

            if (WasCrossbowPressed())
                crossbowBuffer.Press(actionInputBufferDuration);

            if (WasStanceTogglePressed())
                stanceToggleBuffered = true;
        }

#if ENABLE_INPUT_SYSTEM
        private static bool ReadInteractHeld()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.eKey.isPressed)
                return true;

            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.buttonNorth.isPressed;
        }

        private static bool ReadBlockHeld()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null
                && (keyboard.cKey.isPressed || keyboard.leftCtrlKey.isPressed))
                return true;

            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.leftTrigger.ReadValue() > 0.45f;
        }

        private static bool ReadJumpHeld()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.spaceKey.isPressed)
                return true;

            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.buttonSouth.isPressed;
        }

        private static bool WasJumpPressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
                return true;

            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.buttonSouth.wasPressedThisFrame;
        }

        private static bool WasDashPressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame)
                    return true;
            }

            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.buttonEast.wasPressedThisFrame;
        }

        private static bool WasMeleePressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.jKey.wasPressedThisFrame)
                return true;

            if (WasPointerPressedThisFrame(0))
                return true;

            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.buttonWest.wasPressedThisFrame;
        }

        private static bool WasCrossbowPressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
                return true;

            if (WasPointerPressedThisFrame(1))
                return true;

            Gamepad gamepad = Gamepad.current;
            // Right trigger — not buttonNorth (Y), which is Interact.
            return gamepad != null && gamepad.rightTrigger.wasPressedThisFrame;
        }

        private static bool WasStanceTogglePressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.tabKey.wasPressedThisFrame)
                return true;

            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.dpad.up.wasPressedThisFrame;
        }

        private static bool WasPointerPressedThisFrame(int button)
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
                return false;

            return button == 0
                ? mouse.leftButton.wasPressedThisFrame
                : mouse.rightButton.wasPressedThisFrame;
        }

        private PlayerInputFrame ReadMoveFrame()
        {
            Vector2 move = Vector2.zero;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                    move.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                    move.y -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                    move.x += 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                    move.x -= 1f;
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 stick = gamepad.leftStick.ReadValue();
                if (stick.sqrMagnitude > move.sqrMagnitude)
                    move = stick;
            }

            if (move.sqrMagnitude > 1f)
                move.Normalize();

            if (move.magnitude < moveDeadZone)
                move = Vector2.zero;

            return new PlayerInputFrame { Move = move };
        }
#else
        private static bool ReadJumpHeld() => false;
        private static bool ReadInteractHeld() => false;
        private static bool ReadBlockHeld() => false;
        private static bool WasJumpPressed() => false;
        private static bool WasDashPressed() => false;
        private static bool WasMeleePressed() => false;
        private static bool WasCrossbowPressed() => false;
        private static bool WasStanceTogglePressed() => false;

        private PlayerInputFrame ReadMoveFrame() => default;
#endif
    }
}
