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
    /// Samples input in Update; jump/dash are buffered for FixedUpdate consumption (stable physics).
    /// Uses the Input System package only (no legacy UnityEngine.Input calls).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private float moveDeadZone = 0.12f;

        private PlayerInputFrame currentFrame;
        private bool jumpBuffered;
        private bool dashBuffered;
        private bool meleeBuffered;
        private bool crossbowBuffered;
        private bool stanceToggleBuffered;

        public PlayerInputFrame CurrentFrame => currentFrame;
        public bool JumpHeld { get; private set; }
        public bool InteractHeld { get; private set; }

        private void Update()
        {
            currentFrame = ReadMoveFrame();
            JumpHeld = ReadJumpHeld();
            InteractHeld = ReadInteractHeld();
            BufferActions();
        }

        public bool ConsumeJump()
        {
            if (!jumpBuffered)
                return false;

            jumpBuffered = false;
            return true;
        }

        public bool ConsumeDash()
        {
            if (!dashBuffered)
                return false;

            dashBuffered = false;
            return true;
        }

        public bool ConsumeMeleeAttack()
        {
            if (!meleeBuffered)
                return false;

            meleeBuffered = false;
            return true;
        }

        public bool ConsumeCrossbowAttack()
        {
            if (!crossbowBuffered)
                return false;

            crossbowBuffered = false;
            return true;
        }

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
                jumpBuffered = true;

            if (WasDashPressed())
                dashBuffered = true;

            if (WasMeleePressed())
                meleeBuffered = true;

            if (WasCrossbowPressed())
                crossbowBuffered = true;

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
            return gamepad != null && gamepad.buttonNorth.wasPressedThisFrame;
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
        private static bool WasJumpPressed() => false;
        private static bool WasDashPressed() => false;
        private static bool WasMeleePressed() => false;
        private static bool WasCrossbowPressed() => false;
        private static bool WasStanceTogglePressed() => false;

        private PlayerInputFrame ReadMoveFrame() => default;
#endif
    }
}
