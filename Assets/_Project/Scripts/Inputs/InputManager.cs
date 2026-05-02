using UnityEngine;
using UnityEngine.InputSystem;

namespace Speed.Inputs
{
    [RequireComponent(typeof(InputBuffer))]
    public class InputManager : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }

        private InputBuffer _inputBuffer;

        private void Awake()
        {
            _inputBuffer = GetComponent<InputBuffer>();
        }

        // --- These methods should be hooked up via the Unity InputSystem PlayerInput component (Send Messages / Invoke Unity Events) ---

        public void OnMove(InputValue value)
        {
            MoveInput = value.Get<Vector2>();
        }

        public void OnLook(InputValue value)
        {
            LookInput = value.Get<Vector2>();
        }

        public void OnJump(InputValue value)
        {
            // If the button was pressed
            if (value.isPressed)
            {
                _inputBuffer.BufferAction(PlayerInputAction.Jump);
            }
        }

        // Example for future:
        // public void OnDash(InputValue value) { ... BufferAction(PlayerInputAction.Dash) ... }
    }
}
