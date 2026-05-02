using UnityEngine;
using Speed.Inputs;

namespace Speed.Player
{
    public class IdleState : PlayerState
    {
        public IdleState(PlayerController player) : base(player) { }

        public override void Tick()
        {
        }

        public override void FixedTick()
        {
        }
    }
}
