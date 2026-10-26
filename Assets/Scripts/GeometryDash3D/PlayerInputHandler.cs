using UnityEngine;
using UnityEngine.InputSystem;

namespace GeometryDash3D
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Buffer Settings")]
        [SerializeField] private float _jumpBufferDuration = 0.12f;

        private float _jumpBufferCounter = 0f;
        private bool _jumpHeld = false;
        private bool _jumpPressedThisFrame = false;

        public bool IsJumpHeld => _jumpHeld;
        public bool HasBufferedJump => _jumpBufferCounter > 0f;

        private void Update()
        {
            PollInput();

            if (_jumpPressedThisFrame)
            {
                _jumpBufferCounter = _jumpBufferDuration;
            }
            else if (_jumpBufferCounter > 0f)
            {
                _jumpBufferCounter -= Time.deltaTime;
            }
        }

        private void PollInput()
        {
            _jumpPressedThisFrame = false;
            _jumpHeld = false;

            // Keyboard
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.spaceKey.wasPressedThisFrame ||
                    keyboard.upArrowKey.wasPressedThisFrame ||
                    keyboard.wKey.wasPressedThisFrame)
                {
                    _jumpPressedThisFrame = true;
                }

                if (keyboard.spaceKey.isPressed ||
                    keyboard.upArrowKey.isPressed ||
                    keyboard.wKey.isPressed)
                {
                    _jumpHeld = true;
                }
            }

            // Mouse
            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame)
                {
                    _jumpPressedThisFrame = true;
                }
                if (mouse.leftButton.isPressed)
                {
                    _jumpHeld = true;
                }
            }

            // Touchscreen
            var touch = Touchscreen.current;
            if (touch != null)
            {
                if (touch.primaryTouch.press.wasPressedThisFrame)
                {
                    _jumpPressedThisFrame = true;
                }
                if (touch.primaryTouch.press.isPressed)
                {
                    _jumpHeld = true;
                }
            }

            // Gamepad
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                if (gamepad.buttonSouth.wasPressedThisFrame)
                {
                    _jumpPressedThisFrame = true;
                }
                if (gamepad.buttonSouth.isPressed)
                {
                    _jumpHeld = true;
                }
            }
        }

        public void ConsumeJumpBuffer()
        {
            _jumpBufferCounter = 0f;
        }
    }
}
