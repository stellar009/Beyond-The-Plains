using UnityEngine;
using UnityEngine.InputSystem;

namespace BeyondThePlains.InputSystem
{
    public partial class GameInputManager
    {
        private void EnablePlayerHandler()
        {
            m_InputAction.Player.Movement.performed += OnPlayerMovementPerformed;
            m_InputAction.Player.Movement.canceled += OnPlayerMovementCanceled;
            m_InputAction.Player.Sprint.performed += OnSprintPressed;
        }

        private void DisablePlayerHandler()
        {
            m_InputAction.Player.Movement.performed -= OnPlayerMovementPerformed;
            m_InputAction.Player.Movement.canceled -= OnPlayerMovementCanceled;
            m_InputAction.Player.Sprint.performed -= OnSprintPressed;
        }

        void OnPlayerMovementPerformed(InputAction.CallbackContext ctx) => playerMovement = ctx.ReadValue<Vector2>();

        void OnPlayerMovementCanceled(InputAction.CallbackContext ctx) => playerMovement = Vector2.zero;

        void OnSprintPressed(InputAction.CallbackContext ctx) => isSprint = ctx.ReadValueAsButton();

        public void EnableSprint(bool enable)
        {
            if (enable)
            {
                m_InputAction.Player.Sprint.Enable();
            }
            else
            {
                m_InputAction.Player.Sprint.Disable();
            }
        }
    }
}
