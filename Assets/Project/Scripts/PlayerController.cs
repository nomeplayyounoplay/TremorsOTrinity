using Unity.Cinemachine;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;

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
        [SerializeField] float InitialMaxPlayerMoveSpeed = 400f;
        [SerializeField] float minPlayerMoveSpeed = 40f;
        [SerializeField] float playerSpeedModifier = 20f;
        [SerializeField] float rotationSpeed = 500f;
        [SerializeField] float animatorSmoothTime = 0.2f;
        [SerializeField] float playerReach = 0.5f;

        StateMachine stateMachine;
        Transform mainCam;

        float playerHeight;
        float playerRadius;

        //Player Movement Variables
        Vector3 playerMovement;
        float velocity;
        float currentPlayerSpeedModifier;
        float maxPlayerMoveSpeed;

        //Animation
        static readonly int Speed = Animator.StringToHash("Speed");
        static readonly int Stealthed = Animator.StringToHash("Stealthed");
        float animSpeed = 0.0f;

        public bool isTesting = false;
        bool isShiftHeld = false;

        bool isStealthed = false;

        void OnEnable()
        {
            input.TestKeyPressed += OnTestKeyPressed;
            input.TestKeyReleased += OnTestKeyReleased;

            input.StartSpeedChange += OnShiftPressed;
            input.EndSpeedChange += OnShiftReleased;
            input.ChangeSpeed += AdjustMovementSpeed;

            input.ToggleStealth += ToggleStealthMode;

            input.Interact += AttemptInteraction;
        }
        void OnDisable() 
        {
            input.TestKeyPressed -= OnTestKeyPressed;
            input.TestKeyReleased -= OnTestKeyReleased;

            input.StartSpeedChange -= OnShiftPressed;
            input.EndSpeedChange -= OnShiftReleased;

            input.Interact -= AttemptInteraction;

            input.ChangeSpeed -= AdjustMovementSpeed;
        }
        void Awake()
        {
            playerHeight = capsuleCollider.height;
            playerRadius = capsuleCollider.radius;

            currentPlayerSpeedModifier = InitialMaxPlayerMoveSpeed * .5f;
            maxPlayerMoveSpeed = InitialMaxPlayerMoveSpeed;

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
            var stealthState = new StealthState(this, animator);

            //Define Transitions
            At(locomotionState, playerTestState, new FuncPredicate(() => IsTesting()));
            At(playerTestState, locomotionState, new FuncPredicate(() => !IsTesting()));
            At(locomotionState, stealthState, new FuncPredicate(() => IsStealthed()));
            At(stealthState, locomotionState, new FuncPredicate(() => !IsStealthed()));


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

            Debug.Log(maxPlayerMoveSpeed);
        }

        void FixedUpdate()
        {
            stateMachine.FixedUpdate();
            UpdateAnimator();
        }

        void UpdateAnimator()
        {
            animator.SetBool(Stealthed, isStealthed);
            animator.SetFloat(Speed, animSpeed);
        }
        public void HandleMovement()
        {
            Vector3 movementDirection = CalculateMovementDirection();
            if(movementDirection.magnitude > ZEROF)
            {
                HandleRotation(movementDirection);
                HandleHorizontalMovement(movementDirection);

                //properly scales animSpeed to fit into speed modification - walk to run animation blend
                float normalizedSpeed = (movementDirection.magnitude * currentPlayerSpeedModifier) / maxPlayerMoveSpeed;
                animSpeed = SmoothSpeed(normalizedSpeed);
            }
            else
            {
                rb.velocity = new Vector3(ZEROF, rb.velocity.y, ZEROF);
                animSpeed = SmoothSpeed(ZEROF);
            }
        }

        Vector3 CalculateMovementDirection()
        {
            Vector3 camForward = mainCam.forward.normalized;
            camForward.y = 0f;
            Vector3 camRight = mainCam.right.normalized;
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
            Vector3 velocity = movementDirection * currentPlayerSpeedModifier * Time.fixedDeltaTime;
            rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
        }

        float SmoothSpeed(float value)
        {
            return Mathf.SmoothDamp(animSpeed, value, ref velocity, animatorSmoothTime);
        }
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


        //Functions for adjusting speed
        //Hold Shift - then can adjust 
        //Change name - using Shift is bad idea
        //Something about Ready to Adjust Speed idk or can or prepared or or orororrororoo
        void OnShiftPressed() => isShiftHeld = true;
        void OnShiftReleased() => isShiftHeld = false;
        bool IsShiftHeld()
        {
            return isShiftHeld;
        }
        void AdjustMovementSpeed(Vector2 value)
        {
            if(!IsShiftHeld()) return;

            float scrollSpeed = input.GetScrollValue;

            if(scrollSpeed > 0 && currentPlayerSpeedModifier < maxPlayerMoveSpeed)
            {
                currentPlayerSpeedModifier += playerSpeedModifier;
                if(currentPlayerSpeedModifier > maxPlayerMoveSpeed) currentPlayerSpeedModifier = maxPlayerMoveSpeed;
            }
            if(scrollSpeed < 0 && currentPlayerSpeedModifier > minPlayerMoveSpeed)
            {
                currentPlayerSpeedModifier -= playerSpeedModifier;
                if(currentPlayerSpeedModifier < minPlayerMoveSpeed) currentPlayerSpeedModifier = minPlayerMoveSpeed;
            }
        }

        void ToggleStealthMode() => isStealthed = !isStealthed;
        
        bool IsStealthed()
        {
            return isStealthed;
        }

        public void SetMaxMoveSpeed(float value) => maxPlayerMoveSpeed = value;
        
        public float GetInitialMaxMoveSpeed()
        {
            return InitialMaxPlayerMoveSpeed;
        }
        public float GetMaxMoveSpeed()
        {
            return maxPlayerMoveSpeed;
        }
    }
}