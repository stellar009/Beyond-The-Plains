using UnityEngine;
using UnityEngine.InputSystem;

namespace BeyondThePlains.InputSystem
{
    public partial class GameInputManager
    {
        private void EnableCursorHandler() => m_InputAction.Interactions.Cursor.performed += ActivateCursor;

        private void DisableCursorHandler() => m_InputAction.Interactions.Cursor.performed -= ActivateCursor;

        void ActivateCursor(InputAction.CallbackContext ctx)
        {
            GameManager.instance.EnableCursor(ctx.ReadValueAsButton());
        }
    }
}
