using UnityEngine;

namespace Speed.Player
{
    public class JumpState : PlayerState
    {
        public JumpState(PlayerController player) : base(player) { }

        public override void Enter()
        {
            // Set vertical velocity directly upon entering the state
            Vector3 vel = player.rb.linearVelocity;
            vel.y = player.jumpForce;
            player.rb.linearVelocity = vel;
        }

        public override void Tick()
        {
            // Transition to FallState when momentum shifts downwards
            if (player.rb.linearVelocity.y <= 0f)
            {
                player.StateMachine.ChangeState(player.FallState);
            }
        }

        public override void FixedTick()
        {
            // Maintain slight horizontal control in the air if desired
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
            currentVel.x = Mathf.MoveTowards(currentVel.x, targetVelocity.x, player.acceleration * 0.5f * Time.fixedDeltaTime); // Reduced air steering
            currentVel.z = Mathf.MoveTowards(currentVel.z, targetVelocity.z, player.acceleration * 0.5f * Time.fixedDeltaTime);
            player.rb.linearVelocity = currentVel;
        }
    }
}
