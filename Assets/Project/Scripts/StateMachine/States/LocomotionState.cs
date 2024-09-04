using UnityEngine;

namespace RetroHorror
{
    public class LocomotionState : PlayerBaseState
    {
        public LocomotionState(PlayerController player, Animator animator) : base(player, animator){}

        public override void FixedUpdate()
        {
            player.HandleMovement();
        }

        public override void OnEnter()
        {
            animator.CrossFade(LocomotionHash, crossFadeDuration);
        }

    }
}
