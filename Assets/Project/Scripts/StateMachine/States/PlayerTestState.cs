using System;
using UnityEngine;

namespace RetroHorror
{
    public class PlayerTestState : PlayerBaseState
    {
        public PlayerTestState(PlayerController player, Animator animator) : base(player, animator){}

        public override void OnEnter()
        {
            Debug.Log("enter Test");
        }

        public override void Update()
        {
            Debug.Log("update Test");
        }
        public override void FixedUpdate()
        {
            Debug.Log("fixed Test");
        }

        public override void OnExit()
        {
            Debug.Log("exit Test");
        }
    }
}
