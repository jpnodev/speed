using UnityEngine;
using Speed.Inputs;

namespace Speed.Player
{
    public class WalkRunState : PlayerState
    {
        public WalkRunState(PlayerController player) : base(player) { }

        public override void Tick()
        {
            // Check for buffered Jump
            if (player.inputBuffer.Consume(PlayerInputAction.Jump))
            {
                player.StateMachine.ChangeState(player.JumpState);
                return;
            }

            // Return to Idle if no input
            if (player.inputManager.MoveInput.sqrMagnitude < 0.01f)
            {
                player.StateMachine.ChangeState(player.IdleState);
                return;
            }

            // Check if we lost ground
            if (!player.sensors.IsGrounded)
            {
                player.StateMachine.ChangeState(player.FallState);
                return;
            }

            // Check if slope is too steep
            if (player.sensors.CurrentSlopeAngle > player.maxSlopeAngle)
            {
                player.StateMachine.ChangeState(player.SlideState);
                return;
            }
        }

        public override void FixedTick()
        {
            Vector2 input = player.inputManager.MoveInput;

            // Camera relative direction
            Vector3 targetDirection = Vector3.zero;
            if (player.mainCamera != null)
            {
                Vector3 forward = player.mainCamera.forward;
                Vector3 right = player.mainCamera.right;
                forward.y = 0f;
                right.y = 0f;
                forward.Normalize();
                right.Normalize();

                targetDirection = (forward * input.y + right * input.x).normalized;
            }
            else
            {
                targetDirection = new Vector3(input.x, 0, input.y).normalized;
            }

            Vector3 targetVelocity = targetDirection * player.maxSpeed;

            // Apply acceleration
            Vector3 currentVel = player.rb.linearVelocity;

            currentVel.x = Mathf.MoveTowards(currentVel.x, targetVelocity.x, player.acceleration * Time.fixedDeltaTime);
            currentVel.z = Mathf.MoveTowards(currentVel.z, targetVelocity.z, player.acceleration * Time.fixedDeltaTime);

            player.rb.linearVelocity = currentVel;
        }
    }
}
