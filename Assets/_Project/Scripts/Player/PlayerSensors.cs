using UnityEngine;
using Speed.Core;

namespace Speed.Player
{
    public class PlayerSensors : MonoBehaviour
    {
        [Header("Settings")]
        public LayerMask groundLayer;
        public float groundCheckDistance = 1.1f;
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

            // SphereCast instead of Raycast to cover a wider area under the player.
            // Using transform.position as origin (assuming center of player is at y=0 relative to the capsule).
            // The capsule bottom is at -1.0. We want to check slightly past that (groundCheckDistance = 1.1).
            Vector3 origin = transform.position;
            // The distance the sphere needs to travel so its bottom reaches groundCheckDistance.
            float castDistance = groundCheckDistance - groundCheckSphereRadius;

            RaycastHit hit;
            IsGrounded = Physics.SphereCast(origin, groundCheckSphereRadius, Vector3.down, out hit, castDistance, groundLayer);

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

            Vector3 origin = transform.position + Vector3.up * groundCheckSphereRadius;
            float castDistance = groundCheckDistance - groundCheckSphereRadius + 0.1f;

            Gizmos.DrawWireSphere(origin + Vector3.down * castDistance, groundCheckSphereRadius);
        }
    }
}
