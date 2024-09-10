using System;
using Unity.Cinemachine;
using KBCore.Refs;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace RetroHorror
{
    public class PlayerController : ValidatedMonoBehaviour 
    {
        const float ZEROF = 0.0f;

        [Header("References")]
        [SerializeField, Self] Animator animator;
        [SerializeField, Self] Rigidbody rb;
        [SerializeField, Self] CapsuleCollider capsuleCollider;
        [SerializeField, Anywhere] CinemachineCamera freelookCam;
        [SerializeField, Anywhere] InputReader input;
        [SerializeField] LayerMask pickupLayerMask;

        [Header("Setting")]
        [SerializeField] float moveSpeed = 6f;
        [SerializeField] float rotationSpeed = 15f;
        [SerializeField] float animatorSmoothTime = 0.2f;
        [SerializeField] float playerReach = 0.5f;


        StateMachine stateMachine;
        Transform mainCam;

        //Player Movement Variables
        Vector3 playerMovement;
        float velocity;

        //Animation
        static readonly int Speed = Animator.StringToHash("Speed");
        float currentSpeed = 0.0f;

        public bool isTesting = false;

        void OnEnable()
        {
            input.TestKeyPressed += OnTestKeyPressed;
            input.TestKeyReleased += OnTestKeyReleased;

            input.Interact += AttemptInteraction;
        }
        void OnDisable() 
        {
            input.TestKeyPressed -= OnTestKeyPressed;
            input.TestKeyReleased -= OnTestKeyReleased;

            input.Interact -= AttemptInteraction;
        }
        void Awake()
        {
            mainCam = Camera.main.transform;

            //Essentially if the Player teleports/moves away in a flash
            //Makes sure camera does not lose the player 
            freelookCam.OnTargetObjectWarped
            (
                transform,
                transform.position - freelookCam.transform.position - Vector3.forward
            );

            //
           // objectPickup = new ObjectInteraction();

            //StateMachine
            stateMachine = new StateMachine();

            //Declare States
            var playerTestState = new PlayerTestState(this, animator);
            var locomotionState = new LocomotionState(this, animator);

            //Define Transitions
            At(locomotionState, playerTestState, new FuncPredicate(() => IsTesting()));
            At(playerTestState, locomotionState, new FuncPredicate(() => !IsTesting()));

            //Set Initial State
            stateMachine.SetState(locomotionState);
        } 

        //Helper methods to call StateMachine add transition methods
        void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from,to,condition);
        void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to,condition);

        void Update()
        {
            playerMovement = new Vector3(input.Direction.x, 0f, input.Direction.y);
            stateMachine.Update();
            UpdateAnimator();
        }

        void FixedUpdate()
        {
            stateMachine.FixedUpdate();
        }

        void UpdateAnimator() => animator.SetFloat(Speed, currentSpeed);

        public void HandleMovement()
        {
            Vector3 movementDirection = CalculateMovementDirection();
            if(movementDirection.magnitude > ZEROF)
            {
                HandleHorizontalMovement(movementDirection);
                HandleRotation(movementDirection);
                currentSpeed = SmoothSpeed(movementDirection.magnitude);
            }
            else
            {
                rb.velocity = new Vector3(ZEROF, rb.velocity.y, ZEROF);
                currentSpeed = SmoothSpeed(ZEROF);
            }
        }

        Vector3 CalculateMovementDirection()
        {
            Vector3 camForward = mainCam.forward;
            camForward.y = 0f;
            Vector3 camRight = mainCam.right;
            camRight.y = 0f; 
            
            Vector3 movementDirection = playerMovement.z * camForward.normalized + 
                    playerMovement.x * camRight.normalized;
            movementDirection.Normalize();

            return movementDirection;
        }

        void HandleRotation(Vector3 movementDirection)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        void HandleHorizontalMovement(Vector3 movementDirection)
        {
            Vector3 velocity = movementDirection * (moveSpeed * Time.fixedDeltaTime);
            rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
        }

        float SmoothSpeed(float value) => Mathf.SmoothDamp(currentSpeed, value, ref velocity, animatorSmoothTime);

        //Tester - When pressed&held will change states and remain in state until release
        //Use this style for Sneaking
        bool IsTesting()
        {
            return isTesting;
        }
        void OnTestKeyPressed()
        {
            isTesting = true;
            Debug.Log("GO TO TEST!!!");
        }
        void OnTestKeyReleased()
        {
            isTesting = false;
        }

        void AttemptInteraction()
        {
            //Raycast from from Center of Camera in the direction of the mouse position
            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            //Debug.DrawRay(camRay.origin, camRay.direction * 5, Color.red, 20.0f);

            if(Physics.Raycast(camRay, out RaycastHit hitInfo, 200, pickupLayerMask))
            {
                float playerHeight = capsuleCollider.height;
                float playerRadius = capsuleCollider.radius;
                Vector3 directionToObject = hitInfo.point - transform.position;
                if(Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, directionToObject, playerReach))
                {
                    Debug.Log("ASSS");
                }
                else
                {
                    Debug.Log("Object to far");
                }           
            }
        }
    }
}