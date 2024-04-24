using System;
using UnityEngine;
using UnityEngine.InputSystem;
using IPlayerActions = Inputs.Controls.IPlayerActions;

namespace Inputs
{
    [CreateAssetMenu(menuName = "Game/Create InputReader", fileName = "InputReader", order = 0)]
    public class InputReader : ScriptableObject, IPlayerActions
    {
        public event Action<Vector2> MoveEvent; 
        public event Action<bool> PrimaryFireEvent; 
        
        private Controls controls;
        
        public Vector2 AimPosition { get; private set; }
        
        public void OnMove(InputAction.CallbackContext context)
        {
            MoveEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnPrimaryFire(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                PrimaryFireEvent?.Invoke(true);
            }
            else if (context.canceled)
            {
                PrimaryFireEvent?.Invoke(false);
            }
        }
        
        public void OnAim(InputAction.CallbackContext context)
        {
            AimPosition = context.ReadValue<Vector2>();
        }

        private void OnEnable()
        {
            if (controls == null)
            {
                controls = new Controls();
                controls.Player.SetCallbacks(this);
            }
            
            controls.Player.Enable();
        }

        private void OnDisable()
        {
            if (controls != null)
            {
                controls.Player.Disable();
            }
        }
    }
}
