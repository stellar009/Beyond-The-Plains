using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputManager : MonoBehaviour
{
    private InputAction_Game m_InputAction;

    public Vector2 playerMovement { get; private set; }
    public bool nextDialouge { get; private set; }

    public bool isSprint { get; private set; }

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

    void StartTalking(InputAction.CallbackContext ctx)
    {
        FindObjectOfType<DialogueTrigger>().TriggerDialogue();
    }

    void ContinueTalking(InputAction.CallbackContext ctx)
    {
        FindObjectOfType<DialogueSystem>().NextSenetence();
    }

    void ActivateCursor(InputAction.CallbackContext ctx)
    {
        GameManager.instance.EnableCursor(ctx.ReadValueAsButton());
    }

    public void TalkingStarted(bool start)
    {
        if (start)
        {
            m_InputAction.InteractionsDialogue.Talk.Disable();
        }
        else
        {
            m_InputAction.InteractionsDialogue.Talk.Enable();
        }
    }

    public void DialogueCompleted(bool complete)
    {
        if (complete)
        {
            m_InputAction.InteractionsDialogue.ContinueTalk.Disable();
        }
        else
        {
            m_InputAction.InteractionsDialogue.ContinueTalk.Enable();
        }
    }

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

    public void EnableDialogueInteraction(bool enable)
    {
        if(enable)
        {
            m_InputAction.InteractionsDialogue.Enable();

            m_InputAction.InteractionsDialogue.Talk.performed += StartTalking;
            m_InputAction.InteractionsDialogue.ContinueTalk.performed += ContinueTalking;
        }
        else
        {
            m_InputAction.InteractionsDialogue.ContinueTalk.performed -= ContinueTalking;
            m_InputAction.InteractionsDialogue.Talk.performed -= StartTalking;

            m_InputAction.InteractionsDialogue.Disable();
        }
    }
}
