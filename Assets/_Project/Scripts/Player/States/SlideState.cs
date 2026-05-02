using UnityEngine;

namespace Speed.Player
{
    public class SlideState : PlayerState
    {
        public SlideState(PlayerController player) : base(player) { }

        public override void Tick()
        {
            if (!player.sensors.IsGrounded)
            {
                player.StateMachine.ChangeState(player.FallState);
                return;
            }

            // If we are grounded but the slope isn't too steep anymore, back to WalkRun or Idle
            if (player.sensors.CurrentSlopeAngle <= player.maxSlopeAngle)
            {
                if (player.inputManager.MoveInput.sqrMagnitude > 0.01f)
                {
                    player.StateMachine.ChangeState(player.WalkRunState);
                }
                else
                {
                    player.StateMachine.ChangeState(player.IdleState);
                }
                return;
            }

            // Buffered jump out of slide
            if (player.inputBuffer.Consume(Inputs.PlayerInputAction.Jump))
            {
                player.StateMachine.ChangeState(player.JumpState);
                return;
            }
        }

        public override void FixedTick()
        {
            Vector3 currentVel = player.rb.linearVelocity;

            // Calculate downward slope direction
            Vector3 slopeDownDirection = Vector3.Cross(Vector3.Cross(player.sensors.GroundNormal, Vector3.down), player.sensors.GroundNormal).normalized;

            // Slide acceleration
            Vector3 targetSlideVelocity = slopeDownDirection * player.maxSlideSpeed;

            // Keep some lateral input to slightly steer while sliding
            Vector2 input = player.inputManager.MoveInput;
            Vector3 lateralDirection = Vector3.zero;

            if (player.mainCamera != null)
            {
                Vector3 right = player.mainCamera.right;
                right.y = 0;
                right.Normalize();
                lateralDirection = right * input.x;
            }

            // Project lateral steering onto the slope plane
            lateralDirection = Vector3.ProjectOnPlane(lateralDirection, player.sensors.GroundNormal).normalized;

            // Combine slide forward and lateral steer (lateral steer is weaker here)
            Vector3 finalTargetVelocity = targetSlideVelocity + (lateralDirection * player.maxSpeed * 0.5f);

            // Interpolate to our target sliding speed
            currentVel = Vector3.MoveTowards(currentVel, finalTargetVelocity, player.slideAcceleration * Time.fixedDeltaTime);

            player.rb.linearVelocity = currentVel;
        }
    }
}
