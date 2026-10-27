// ---------------------------------------------------------------------------
// DialogueEvents.cs
// ---------------------------------------------------------------------------
// Central event hub. UI panels and gameplay systems subscribe to these
// events instead of polling DialogueManager, which keeps coupling low and
// makes it trivial to add new listeners (achievements, analytics, etc.)
// without touching the core manager.
// ---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using InkDialogueSystem.Data;

namespace InkDialogueSystem.Core
{
    /// <summary>
    /// Static event bus for the dialogue system. Decouples the
    /// DialogueManager from its listeners (UI, save, game logic).
    /// </summary>
    public static class DialogueEvents
    {
        /// <summary>Fired when a new line of dialogue is ready to be shown.
        /// The TypewriterEffect and HistoryPanel both subscribe.</summary>
        public static event Action<DialogueLine> OnLineDisplayed;

        /// <summary>Fired when the set of available choices changes. Passes
        /// an empty list when the choice block ends (player picked one or
        /// story moved on without choices).</summary>
        public static event Action<List<DialogueChoice>> OnChoicesUpdated;

        /// <summary>Fired when the player selects a choice. Useful for
        /// analytics ("which branch did the player take?") and
        /// achievements.</summary>
        public static event Action<DialogueChoice> OnChoiceSelected;

        /// <summary>Fired when the story reaches a #save tag. The save
        /// system can auto-persist here. The argument is the save slot
        /// name (default "auto").</summary>
        public static event Action<string> OnSaveTag;

        /// <summary>Fired when the story ends (story.canContinue == false
        /// AND no choices available). Game flow can advance to the next
        /// scene here.</summary>
        public static event Action OnDialogueEnded;

        /// <summary>Fired when a variable in the Ink story changes. The
        /// VariableInspectorPanel subscribes to refresh its display.
        /// Argument is the variable name that changed.</summary>
        public static event Action<string> OnVariableChanged;

        // --- Dispatch helpers (called only by DialogueManager) ---------

        public static void LineDisplayed(DialogueLine line) => OnLineDisplayed?.Invoke(line);

        public static void ChoicesUpdated(List<DialogueChoice> choices) => OnChoicesUpdated?.Invoke(choices);

        public static void ChoiceSelected(DialogueChoice choice) => OnChoiceSelected?.Invoke(choice);

        public static void SaveTag(string slotName) => OnSaveTag?.Invoke(slotName);

        public static void DialogueEnded() => OnDialogueEnded?.Invoke();

        public static void VariableChanged(string varName) => OnVariableChanged?.Invoke(varName);
    }
}
