using UnityEngine;
using Speed.Core;

namespace Speed.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerSensors : MonoBehaviour
    {
        [Header("Settings")]
        public LayerMask groundLayer;
        public float groundCheckDistance = 1.5f; // Increased to 1.5 to reach steep slopes!
        public float groundCheckSphereRadius = 0.4f; // Radius of the sphere cast

        [Header("Status (Read Only)")]
        public bool IsGrounded;
        public Vector3 GroundNormal; // Storing normal to handle slopes later if needed
        public float CurrentSlopeAngle { get; private set; }

        private PlayerController _player;
        private bool _wasGroundedLastFrame;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
        }

        private void FixedUpdate()
        {
            _wasGroundedLastFrame = IsGrounded;

            // Simplified, highly reliable SphereCast.
            // Capsule radius is 0.5. We use 0.45 to prevent getting caught on side-walls.
            float radius = 0.45f;
            // The capsule's center is 0. Bottom is -1.0.
            // A sphere of 0.45 needs to travel 0.55 down to hit exactly -1.0. We add 0.1 buffer to check floor.
            float castDistance = 0.65f;

            IsGrounded = Physics.SphereCast(transform.position, radius, Vector3.down, out RaycastHit hit, castDistance, groundLayer);

            if (IsGrounded)
            {
                GroundNormal = hit.normal;
                CurrentSlopeAngle = Vector3.Angle(Vector3.up, GroundNormal);
            }
            else
            {
                GroundNormal = Vector3.up;
                CurrentSlopeAngle = 0f;
            }

            // Emit 'Landed' event if transition from airborne to grounded
            if (IsGrounded && !_wasGroundedLastFrame)
            {
                _player.EventQueue.AddEvent(GameplayEventType.Landed, priority: 10);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.down * 0.65f, 0.45f);
        }
    }
}
