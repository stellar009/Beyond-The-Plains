using DialogueSystem.Core;
using DialogueSystem.Essentials;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputManager : MonoBehaviour
{
    private InputAction_Game m_InputAction;
    private DialogueManager m_DialogueManager;
    private DialogueTrigger m_DialogueTrigger;

    public Vector2 playerMovement { get; private set; }

    public bool isSprint { get; private set; }

    public bool skipWriterEffect { get; private set; }

    private void Awake()
    {
        m_InputAction = new InputAction_Game();
    }

    private void Start()
    {
        m_DialogueManager = FindAnyObjectByType<DialogueManager>();
        m_DialogueTrigger = FindAnyObjectByType<DialogueTrigger>();
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

    #region Key Maps
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

    void SkipTypeEffect(InputAction.CallbackContext ctx)
    {
        skipWriterEffect = !skipWriterEffect;
    }

    void StartDialogue(InputAction.CallbackContext ctx)
    {
        m_DialogueManager.StartDialogue(m_DialogueTrigger.thoughts);
    }

    void NextDialogue(InputAction.CallbackContext ctx)
    {
        m_DialogueManager.DisplayNextSentence();
    }
    #endregion

    #region Custom Properties
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

    public void EnableDialogueInput(bool  enable)
    {
        if(enable)
        {
            m_InputAction.DialogueSystem.Enable();

            m_InputAction.DialogueSystem.StopTextEffect.performed += SkipTypeEffect;
            m_InputAction.DialogueSystem.StartThinking.performed += StartDialogue;
            m_InputAction.DialogueSystem.NextDialogue.performed += NextDialogue;
        }
        else
        {
            m_InputAction.DialogueSystem.NextDialogue.performed -= NextDialogue;
            m_InputAction.DialogueSystem.StartThinking.performed -= StartDialogue;
            m_InputAction.DialogueSystem.StopTextEffect.performed -= SkipTypeEffect;

            m_InputAction.DialogueSystem.Disable();
        }
    }
    #endregion
}
