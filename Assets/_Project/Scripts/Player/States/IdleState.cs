using UnityEngine;
using Speed.Inputs;

namespace Speed.Player
{
    public class IdleState : PlayerState
    {
        public IdleState(PlayerController player) : base(player) { }

        public override void Tick()
        {
            // Check for buffered Jump
            if (player.inputBuffer.Consume(PlayerInputAction.Jump))
            {
                player.StateMachine.ChangeState(player.JumpState);
                return;
            }

            // Check if player begins moving
            if (player.inputManager.MoveInput.sqrMagnitude > 0.01f)
            {
                player.StateMachine.ChangeState(player.WalkRunState);
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
            // Gradually decelerate to a stop
            Vector3 vel = player.rb.linearVelocity;
            vel.x = Mathf.MoveTowards(vel.x, 0, player.acceleration * Time.fixedDeltaTime);
            vel.z = Mathf.MoveTowards(vel.z, 0, player.acceleration * Time.fixedDeltaTime);
            player.rb.linearVelocity = vel;
        }
    }
}
