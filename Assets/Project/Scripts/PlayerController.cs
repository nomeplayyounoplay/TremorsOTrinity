using Unity.Cinemachine;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.InputSystem;

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

        float playerHeight;
        float playerRadius;

        //Player Movement Variables
        Vector3 playerMovement;
        float velocity;

        //Animation
        static readonly int Speed = Animator.StringToHash("Speed");
        float currentSpeed = 0.0f;

        public bool isTesting = false;

        bool isShiftHeld = false;

        void OnEnable()
        {
            input.TestKeyPressed += OnTestKeyPressed;
            input.TestKeyReleased += OnTestKeyReleased;

            input.ShiftPressed += OnShiftPressed;
            input.ShiftReleased += OnShiftReleased;

            input.Interact += AttemptInteraction;

        }
        void OnDisable() 
        {
            input.TestKeyPressed -= OnShiftPressed;
            input.TestKeyReleased -= OnTestKeyReleased;

            input.ShiftPressed -= OnShiftPressed;
            input.ShiftReleased -= OnShiftReleased;

            input.Interact -= AttemptInteraction;
        }
        void Awake()
        {
            playerHeight = capsuleCollider.height;
            playerRadius = capsuleCollider.radius;

            mainCam = Camera.main.transform;

            //Essentially if the Player teleports/moves away in a flash
            //Makes sure camera does not lose the player 
            freelookCam.OnTargetObjectWarped
            (
                transform,
                transform.position - freelookCam.transform.position - Vector3.forward
            );

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
            AdjustMovementSpeed();
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
                HandleRotation(movementDirection);
                HandleHorizontalMovement(movementDirection);
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
            Vector3 camForward = (mainCam.forward).normalized;
            camForward.y = 0f;
            Vector3 camRight = (mainCam.right).normalized;
            camRight.y = 0f; 
            
            Vector3 movementDirection = playerMovement.z * camForward + 
                    playerMovement.x * camRight;
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
        void OnTestKeyPressed() => isTesting = true;
        void OnTestKeyReleased() => isTesting = false;
        bool IsTesting()
        {
            return isTesting;
        }

        void AttemptInteraction()
        {
            //When interact capsulecast from the player forward in the direction theyre facing
            //if hit interactable do da interact dawg
            if(Physics.CapsuleCast
                (
                    transform.position,                             //bottom of capsule
                    transform.position + Vector3.up * playerHeight, //top of capsule
                    playerRadius, 
                    transform.forward, 
                    out RaycastHit capsuleHit,
                    playerReach,
                    pickupLayerMask
                )
            )
            {
                Interactable hitInfo = capsuleHit.transform.GetComponent<Interactable>();
                if(hitInfo != null)
                {
                    hitInfo.Interact();
                }
            }
            else
            {
                Debug.Log("Nothing to interact with");
            } 
        }

        void AdjustMovementSpeed()
        {
            if(!IsShiftHeld()) return;

            float scrollSpeed = input.GetScrollValue;
            print(scrollSpeed);
        }
        
        void OnShiftPressed() => isShiftHeld = true;
        void OnShiftReleased() => isShiftHeld = false;
        bool IsShiftHeld()
        {
            return isShiftHeld;
        }

    }
}