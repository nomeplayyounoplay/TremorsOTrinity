using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace RetroHorror
{
    //Allows us to create an aszset out of this ScritableObject for use in the editor
    //We attach it to PlayerController which is attached to Player
    [CreateAssetMenu(fileName = "InputReader", menuName = "RetroHorror/InputReader")]

    public class InputReader : ScriptableObject, PlayerInputActions.IPlayerActions
    {
        public event UnityAction<Vector2> Move = delegate {};
        public event UnityAction Interact = delegate {};
        public event UnityAction<Vector2> ChangeSpeed = delegate {};
        public event UnityAction ShiftPressed = delegate {};
        public event UnityAction ShiftReleased = delegate {};
        public event UnityAction TestKeyPressed = delegate {};
        public event UnityAction TestKeyReleased = delegate {};
        PlayerInputActions inputActions;

        //Below the OnMove function stores the value 
        //Direction reads the latest value stored by OnMove and returns it to us as a Vec3 with Z=0
        //Convenient way to get current direction as vec3
        public Vector3 Direction => (Vector3)inputActions.Player.Move.ReadValue<Vector2>();
        public float GetScrollValue => inputActions.Player.ChangeSpeed.ReadValue<Vector2>().y;
        
        void OnEnable()
        {
            if(inputActions == null)
            {
                inputActions = new PlayerInputActions();
                inputActions.Player.SetCallbacks(this);
            }
            inputActions.Enable();
        }
        //When the PlayerController Moves - WASD or Arrows as we set in Editor
        //It will trigger OnMove
        public void OnMove(InputAction.CallbackContext context) => Move?.Invoke(context.ReadValue<Vector2>());

        public void OnTestKey(InputAction.CallbackContext context)
        {
            switch(context.phase)
            {
                case InputActionPhase.Performed:
                    TestKeyPressed?.Invoke();
                    break;
                case InputActionPhase.Canceled:
                    TestKeyReleased?.Invoke();
                    break;
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if(context.performed)
            {
                Interact?.Invoke();
            }
        }

        public void OnChangeSpeed(InputAction.CallbackContext context) => ChangeSpeed?.Invoke(context.ReadValue<Vector2>());

        public void OnStartSpeedChange(InputAction.CallbackContext context)
        {
            switch(context.phase)
            {
                case InputActionPhase.Performed:
                    ShiftPressed?.Invoke();
                    break;
                case InputActionPhase.Canceled:
                    ShiftReleased?.Invoke();
                    break;
            }
        }
    }
}
