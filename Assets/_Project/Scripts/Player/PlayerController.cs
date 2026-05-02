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
        public MoveState MoveState { get; private set; }
        public MachState MachState { get; private set; }
        public BrakeState BrakeState { get; private set; }
        public JumpState JumpState { get; private set; }
        public JumpSlideState JumpSlideState { get; private set; }
        public JumpMachState JumpMachState { get; private set; }
        public FallState FallState { get; private set; }
        public SlideState SlideState { get; private set; }

        [Header("Movement Stats - Speed Thresholds")]
        public float maxMoveSpeed = 10f;           // Walking/running cap
        public float machSpeedThreshold = 15f;     // When you enter MachState
        public float absoluteMaxSpeed = 20f;       // Hard ceiling for safety
        public float stickThreshold = 0.5f;        // Below = fixed walk, above = accelerate
        public float maxWalkSpeed = 5f;            // Fixed speed when stick is below threshold
        public float maxSlidingSlope = 60f;        // Angle at which slide cruise speed reaches maximum

        [Header("Movement Stats - Acceleration & Friction")]
        public float moveAcceleration = 50f;       // Ground acceleration in MoveState
        public float machAcceleration = 30f;       // Reduced acceleration in MachState (momentum dominant)
        public float groundFriction = 0.95f;       // Speed bleed per frame when coasting on ground
        public float airFriction = 0.98f;          // Much lighter drag in air
        public float brakeFriction = 0.8f;         // Hard braking deceleration
        public float jumpForce = 15f;
        public float customGravityMultiplier = 3f;
        public float maxFallSpeed = 40f;           // Terminal velocity to prevent clipping
        public float slideStartSlopeAngle = 33f;    // Angles steeper than this will trigger sliding
        // public float slideAcceleration = 20f;      // How fast the player accelerates while sliding
        // calculate slope acceleration based on slope angle, custom gravity, and mass for a more natural feel. We can still tweak it with a multiplier.
        public float slideAccelerationMultiplier = 1.5f; // Multiplier to tweak natural slope acceleration
        // public float maxSlideSpeed = 25f;
        // Max slide speed depends on slope angle, with a multiplier for tuning. Steeper slopes allow higher slide speeds, while shallower slopes have a lower max speed to preserve momentum without becoming unmanageable.
        // Going into shallower slopes from steep ones should feel smooth and natural, without a jarring drop in speed, so momentum in slide state is preserved until the slope angle drops below slideStartSlopeAngle, at which point we start reducing the max slide speed to eventually converge with maxWalkSpeed at 0 degrees.

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            // Turn off Unity's automated gravity so we have absolute control over slopes and falling
            rb.useGravity = false;


            StateMachine = new StateMachine();
            EventQueue = new GameplayEventQueue();

            inputBuffer = gameObject.GetComponent<InputBuffer>();
            if (inputBuffer == null) inputBuffer = gameObject.AddComponent<InputBuffer>();

            inputManager = gameObject.GetComponent<InputManager>();
            if (inputManager == null) inputManager = gameObject.AddComponent<InputManager>();

            // Initialize states
            IdleState = new IdleState(this);
            MoveState = new MoveState(this);
            MachState = new MachState(this);
            BrakeState = new BrakeState(this);
            JumpState = new JumpState(this);
            JumpSlideState = new JumpSlideState(this);
            JumpMachState = new JumpMachState(this);
            FallState = new FallState(this);
            SlideState = new SlideState(this);

            if (mainCamera == null && Camera.main != null)
            {
                mainCamera = Camera.main.transform;
            }
        }

        public float GetSlopeSteepness01(float slopeAngle)
        {
            // Map slope angle range [slideStartSlopeAngle, maxSlidingSlope] to [0, 1]
            // At slideStartSlopeAngle: steepness = 0 (entry point, minimal momentum)
            // At maxSlidingSlope: steepness = 1 (maximum slide cruise speed)
            return Mathf.InverseLerp(slideStartSlopeAngle, maxSlidingSlope, slopeAngle);
        }

        public float GetSlopeAwareMachThreshold(float slopeAngle)
        {
            float steepness = GetSlopeSteepness01(slopeAngle);
            return Mathf.Lerp(machSpeedThreshold, absoluteMaxSpeed * 0.85f, steepness);
        }

        public float GetSlopeAwareSlideCruiseSpeed(float slopeAngle)
        {
            float steepness = GetSlopeSteepness01(slopeAngle);
            // At slideStartSlopeAngle: cruise speed = maxWalkSpeed (preserve momentum on barely-steep slopes)
            // At maxSlidingSlope: cruise speed = maxSlideSpeed (peak slide speed)
            return Mathf.Lerp(maxWalkSpeed, maxSlideSpeed, steepness);
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
