using UnityEngine;
using Speed.Core;
using Speed.Player;

namespace Speed.CameraSystem
{
    public class CameraManager : MonoBehaviour
    {
        [Header("References")]
        public Transform target; // The player to follow
        public Camera mainCamera;

        [Header("State Machine")]
        public StateMachine UnderlyingStateMachine { get; private set; } // We can reuse our Core StateMachine! ... actually Core.State doesn't map directly to CameraState conceptually if we want LateTick.
        // Wait, Core.State has Tick and FixedTick. Let's make a mini StateMachine for the Camera or just adopt Core.State.
        // I will use a simple internal state manager to keep it clean.

        public CameraState CurrentState { get; private set; }

        // Concrete States
        public OrbitCameraState OrbitState { get; private set; }
        public FixedCameraState FixedState { get; private set; }

        [Header("Orbit Settings")]
        public float lookSensitivity = 2f;
        public float distance = 5f;
        public float minVerticalAngle = -20f;
        public float maxVerticalAngle = 70f;
        public Vector3 targetOffset = new Vector3(0, 1f, 0); // Offset from the player's pivot

        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            // Initialize states
            OrbitState = new OrbitCameraState(this);
            FixedState = new FixedCameraState(this); // For future
        }

        private void Start()
        {
            ChangeState(OrbitState);

            // Lock cursor for a 3rd person game
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void ChangeState(CameraState newState)
        {
            if (CurrentState == newState) return;

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }

        private void LateUpdate()
        {
            CurrentState?.LateTick();
        }
    }
}
