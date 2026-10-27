// ---------------------------------------------------------------------------
// DialogueDirector.cs
// ---------------------------------------------------------------------------
// Lightweight demo director. Drives a typical narrative game session:
//   1. Loads a story by name.
//   2. Continues the first few lines automatically so the player sees
//      something on screen immediately.
//   3. Wires the global Continue/SelectChoice inputs (any-key-advance
//      when no choice is pending, ESC to toggle history, F1 to toggle
//      variable inspector, F5 to quicksave, F9 to quickload).
//
// Drop this on a GameObject in your scene along with DialogueManager,
// DialogueSaveSystem, and the UI prefab. It's the glue that ties
// everything together for the sample scene. Real games often fold
// these responsibilities into a dedicated GameSession state machine;
// that's fine -- this script exists to make the demo scene plug-and-play.
// ---------------------------------------------------------------------------

using UnityEngine;
using InkDialogueSystem.Core;
using InkDialogueSystem.UI;

namespace InkDialogueSystem.Core
{
    /// <summary>
    /// Sample scene director. Wires global hotkeys and kicks off the
    /// story. Replace or extend as your game flow requires.
    /// </summary>
    public class DialogueDirector : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private DialogueSaveSystem saveSystem;
        [SerializeField] private DialogueHistoryPanel historyPanel;
        [SerializeField] private VariableInspectorPanel variableInspectorPanel;

        [Header("Story")]
        [SerializeField] private string storyName = "sample_story";

        [Header("Input")]
        [SerializeField] private KeyCode advanceKey = KeyCode.Return;
        [SerializeField] private KeyCode historyToggleKey = KeyCode.H;
        [SerializeField] private KeyCode inspectorToggleKey = KeyCode.F1;
        [SerializeField] private KeyCode quicksaveKey = KeyCode.F5;
        [SerializeField] private KeyCode quickloadKey = KeyCode.F9;
        [SerializeField] private bool advanceOnAnyMouseClick = true;

        private void Start()
        {
            if (dialogueManager == null) dialogueManager = DialogueManager.Instance;
            if (dialogueManager != null && dialogueManager.IsRunning == false)
            {
                dialogueManager.StartStory(storyName);
                // Pump the first line so the UI is populated immediately.
                dialogueManager.Continue();
            }
        }

        private void Update()
        {
            if (dialogueManager == null) return;

            // F1: variable inspector
            if (Input.GetKeyDown(inspectorToggleKey) && variableInspectorPanel != null)
                variableInspectorPanel.Toggle();

            // H: history
            if (Input.GetKeyDown(historyToggleKey) && historyPanel != null)
                historyPanel.Toggle();

            // F5/F9: quicksave / quickload
            if (Input.GetKeyDown(quicksaveKey) && saveSystem != null)
            {
                saveSystem.Save("quicksave");
                Debug.Log("[DialogueDirector] Quicksaved.");
            }
            if (Input.GetKeyDown(quickloadKey) && saveSystem != null)
            {
                saveSystem.Load("quicksave");
                Debug.Log("[DialogueDirector] Quickloaded.");
            }

            // Advance on Enter or any mouse click (but only when no
            // choice is pending -- choices must be clicked explicitly).
            if (!dialogueManager.IsAwaitingChoice)
            {
                if (Input.GetKeyDown(advanceKey) ||
                    (advanceOnAnyMouseClick && Input.GetMouseButtonDown(0)))
                {
                    dialogueManager.Continue();
                }
            }
        }
    }
}
