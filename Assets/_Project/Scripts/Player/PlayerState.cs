using Speed.Core;

namespace Speed.Player
{
    public abstract class PlayerState : State
    {
        protected PlayerController player;

        public PlayerState(PlayerController player)
        {
            this.player = player;
        }
    }
}
