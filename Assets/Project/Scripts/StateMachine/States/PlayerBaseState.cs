using UnityEngine;

namespace RetroHorror
{
    public abstract class PlayerBaseState : BaseState
    {
        protected static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
        protected static readonly int StealthHash = Animator.StringToHash("Stealth");
        protected readonly PlayerController player;
        protected readonly Animator animator;
        protected const float crossFadeDuration = 0.1f;
        protected PlayerBaseState(PlayerController player, Animator animator)
        {
            this.player = player;
            this.animator = animator;
        }
    }
}
