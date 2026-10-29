using Mono.Cecil.Cil;
using UnityEngine.InputSystem;

namespace BeyondThePlains.InputSystem
{
    public partial class GameInputManager
    {
        public void EnableDialogueInput(bool enable)
        {
            var dialogue = m_InputAction.DialogueSystem;
            if (enable)
            {
                dialogue.Enable();

                dialogue.StopTextEffect.performed += SkipTypeEffect;
                dialogue.NextDialogue.performed += NextDialogue;
            }
            else
            {
                dialogue.NextDialogue.performed -= NextDialogue;
                dialogue.StopTextEffect.performed -= SkipTypeEffect;

                dialogue.Disable();
            }
        }

        void NextDialogue(InputAction.CallbackContext ctx) => m_DialogueManager.DisplayNextSentence();
        void SkipTypeEffect(InputAction.CallbackContext ctx) => skipWriterEffect = !skipWriterEffect;
    }
}
