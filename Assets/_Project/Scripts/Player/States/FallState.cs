using UnityEngine;
using Speed.Core;
using Speed.Inputs;

namespace Speed.Player
{
    public class FallState : PlayerState
    {
        public FallState(PlayerController player) : base(player) { }

        public override void Tick()
        {
            // Check for Landing Event
            if (player.EventQueue.TryConsumeEvent(GameplayEventType.Landed))
            {
                if (player.sensors.CurrentSlopeAngle > player.maxSlopeAngle)
                {
                    player.StateMachine.ChangeState(player.SlideState);
                    return;
                }

                // Smooth transition depending on whether user is holding input or not
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

            // Allow double jump buffer here if we add early jump while falling close to ground
            // The buffer itself will hold onto it if player presses Jump early
        }

        public override void FixedTick()
        {
            // Air steering
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

            Vector3 currentVel = player.rb.linearVelocity;
            currentVel.x = Mathf.MoveTowards(currentVel.x, targetVelocity.x, player.acceleration * 0.5f * Time.fixedDeltaTime);
            currentVel.z = Mathf.MoveTowards(currentVel.z, targetVelocity.z, player.acceleration * 0.5f * Time.fixedDeltaTime);

            // Custom gravity application
            currentVel.y += Physics.gravity.y * player.customGravityMultiplier * Time.fixedDeltaTime;

            // Terminal velocity clamping
            currentVel.y = Mathf.Max(currentVel.y, -player.maxFallSpeed);

            player.rb.linearVelocity = currentVel;
        }
    }
}
