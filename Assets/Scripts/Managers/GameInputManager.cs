using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputManager : MonoBehaviour
{
    private InputAction_Game m_InputAction;

    public Vector2 playerMovement { get; private set; }
    public bool nextDialouge { get; private set; }

    private void Awake()
    {
        m_InputAction = new InputAction_Game();
    }

    private void OnEnable()
    {
        m_InputAction.Player.Enable();
        m_InputAction.Interactions.Enable();

        m_InputAction.Player.Movement.performed += OnPlayerMovementPerformed;
        m_InputAction.Player.Movement.canceled += OnPlayerMovementCanceled;

        m_InputAction.Interactions.Cursor.performed += ActivateCursor;

        m_InputAction.Quest.Enable();
        m_InputAction.Quest.NextDialouge.performed += NextDialouge;
    }

    private void OnDisable()
    {
        m_InputAction.Interactions.Cursor.performed -= ActivateCursor;

        m_InputAction.Player.Movement.performed -= OnPlayerMovementPerformed;
        m_InputAction.Player.Movement.canceled -= OnPlayerMovementCanceled;

        m_InputAction.Player.Disable();
        m_InputAction.Interactions.Disable();

        m_InputAction.Quest.NextDialouge.performed -= NextDialouge;
        m_InputAction.Quest.Disable();
    }

    void OnPlayerMovementPerformed(InputAction.CallbackContext ctx)
    {
        playerMovement = ctx.ReadValue<Vector2>();
    }

    void OnPlayerMovementCanceled(InputAction.CallbackContext ctx)
    {
        playerMovement = Vector2.zero;
    }

    void NextDialouge(InputAction.CallbackContext ctx)
    {
        nextDialouge = !nextDialouge;
    }

    void ActivateCursor(InputAction.CallbackContext ctx)
    {
        GameManager.instance.EnableCursor(ctx.ReadValueAsButton());
    }
}
