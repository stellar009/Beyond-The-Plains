using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputManager : MonoBehaviour
{
    private InputAction_Game m_InputAction;

    public Vector2 playerMovement { get; private set; }

    public bool isSprint { get; private set; }

    private void Awake()
    {
        m_InputAction = new InputAction_Game();
    }

    #region Key Subscriptions
    private void OnEnable()
    {
        m_InputAction.Player.Enable();
        m_InputAction.Interactions.Enable();

        m_InputAction.Player.Movement.performed += OnPlayerMovementPerformed;
        m_InputAction.Player.Movement.canceled += OnPlayerMovementCanceled;
        m_InputAction.Player.Sprint.performed += OnSprintPressed;

        m_InputAction.Interactions.Cursor.performed += ActivateCursor;
    }

    private void OnDisable()
    {
        m_InputAction.Interactions.Cursor.performed -= ActivateCursor;

        m_InputAction.Player.Movement.performed -= OnPlayerMovementPerformed;
        m_InputAction.Player.Movement.canceled -= OnPlayerMovementCanceled;
        m_InputAction.Player.Sprint.performed -= OnSprintPressed;

        m_InputAction.Player.Disable();
        m_InputAction.Interactions.Disable();
    }
    #endregion

    #region Key Mapping
    void OnPlayerMovementPerformed(InputAction.CallbackContext ctx)
    {
        playerMovement = ctx.ReadValue<Vector2>();
    }

    void OnPlayerMovementCanceled(InputAction.CallbackContext ctx)
    {
        playerMovement = Vector2.zero;
    }

    void OnSprintPressed(InputAction.CallbackContext ctx)
    {
        isSprint = !isSprint;
    }

    void ActivateCursor(InputAction.CallbackContext ctx)
    {
        GameManager.instance.EnableCursor(ctx.ReadValueAsButton());
    }
    #endregion

    #region Custom Functions
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
    #endregion
}
