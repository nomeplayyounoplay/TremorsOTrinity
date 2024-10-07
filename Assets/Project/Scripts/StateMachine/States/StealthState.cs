using UnityEngine;

namespace RetroHorror
{
    public class StealthState : PlayerBaseState
    {
        public StealthState(PlayerController player, Animator animator) : base(player, animator){}

        public override void OnEnter()
        {
            animator.CrossFade(LocomotionHash, crossFadeDuration - 0.02f);
            player.SetMaxMoveSpeed(player.GetMaxMoveSpeed()/ 50);

            player.GetComponent<CapsuleCollider>().height = 0.02f;
        }
        public override void FixedUpdate()
        {
            player.HandleMovement();
        }
        public override void OnExit()
        {
            player.SetMaxMoveSpeed(player.GetInitialMaxMoveSpeed());
            player.GetComponent<CapsuleCollider>().height = 1.78f;
        }
    }

}