using UnityEngine;
using Speed.Core;
using Speed.Inputs;

namespace Speed.Player
{
    [RequireComponent(typeof(Rigidbody), typeof(PlayerSensors))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Components")]
        public Rigidbody rb;
        public PlayerSensors sensors;
        public Transform visuals; // The visual mesh to rotate
        public Transform mainCamera; // Camera used for relative movement

        // Modules
        public StateMachine StateMachine { get; private set; }
        public GameplayEventQueue EventQueue { get; private set; }
        public InputBuffer inputBuffer;
        public InputManager inputManager;

        // Concrete States
        public IdleState IdleState { get; private set; }
        public WalkRunState WalkRunState { get; private set; }
        public JumpState JumpState { get; private set; }
        public FallState FallState { get; private set; }
        public SlideState SlideState { get; private set; }

        [Header("Movement Stats")]
        public float maxSpeed = 10f;
        public float acceleration = 50f;
        public float jumpForce = 15f;
        public float customGravityMultiplier = 3f;
        public float visualRotationSpeed = 15f;
        public float maxFallSpeed = 40f; // Terminal velocity to prevent clipping
        public float maxSlopeAngle = 45f; // Angles steeper than this will trigger sliding
        public float slideAcceleration = 20f; // How fast the player accelerates while sliding
        public float maxSlideSpeed = 25f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            // Enforce Continuous Collision Detection to prevent clipping through the floor at high speeds
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            sensors = GetComponent<PlayerSensors>();

            StateMachine = new StateMachine();
            EventQueue = new GameplayEventQueue();

            inputBuffer = gameObject.GetComponent<InputBuffer>();
            if (inputBuffer == null) inputBuffer = gameObject.AddComponent<InputBuffer>();

            inputManager = gameObject.GetComponent<InputManager>();
            if (inputManager == null) inputManager = gameObject.AddComponent<InputManager>();

            // Initialize states
            IdleState = new IdleState(this);
            WalkRunState = new WalkRunState(this);
            JumpState = new JumpState(this);
            SlideState = new SlideState(this);

            if (mainCamera == null && Camera.main != null)
            {
                mainCamera = Camera.main.transform;
            }
            FallState = new FallState(this);
        }

        private void Start()
        {
            StateMachine.Initialize(IdleState);
        }

        private void Update()
        {
            // Input recording will happen here or in InputManager

            // Rotate visuals
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (visuals == null) return;

            // We want to face the direction we are moving horizontally
            Vector3 movement = rb.linearVelocity;
            movement.y = 0;

            if (movement.sqrMagnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement.normalized);
                visuals.rotation = Quaternion.Slerp(visuals.rotation, targetRotation, visualRotationSpeed * Time.deltaTime);
            }

            StateMachine.Tick();

            // Clean up old momentary events
            EventQueue.ClearOldEvents(0.1f);
        }

        private void FixedUpdate()
        {
            StateMachine.FixedTick();
        }
    }
}
