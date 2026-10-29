using DialogueSystem.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeyondThePlains.InputSystem
{
    public partial class GameInputManager : MonoBehaviour
    {
        private InputAction_Game m_InputAction;
        private DialogueManager m_DialogueManager;

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
        }

        private void OnEnable()
        {
            m_InputAction.Player.Enable();
            m_InputAction.Interactions.Enable();

            EnablePlayerHandler();
            EnableCursorHandler();
        }

        private void OnDisable()
        {
            
            DisableCursorHandler();
            DisablePlayerHandler();

            m_InputAction.Player.Disable();
            m_InputAction.Interactions.Disable();
        }
    }
}
