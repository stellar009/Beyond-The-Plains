// ---------------------------------------------------------------------------
// DialogueChoice.cs
// ---------------------------------------------------------------------------
// Mirror of Ink's Choice object, decoupled from the Ink runtime so the UI
// layer can hold onto choices without dragging the entire runtime into UI
// code. The index field is what the DialogueManager uses to call
// story.ChooseChoiceIndex() when the player clicks.
// ---------------------------------------------------------------------------

using System;

namespace InkDialogueSystem.Data
{
    /// <summary>
    /// A single selectable choice presented to the player at a branch in
    /// the Ink story. Built from Ink.Runtime.Choice.
    /// </summary>
    [Serializable]
    public class DialogueChoice
    {
        /// <summary>The index the Ink runtime expects when the player picks
        /// this choice. Pass this to DialogueManager.SelectChoice().</summary>
        public int index;

        /// <summary>The human-readable text of the choice, already evaluated
        /// by the Ink runtime (variables substituted, conditions resolved).</summary>
        public string text;

        /// <summary>Tags that were on the choice itself (rarely used, but
        /// supported for things like #once or #hidden metadata).</summary>
        public string[] tags;

        /// <summary>True if this choice leads back to the same knot/stitch
        /// (a "loop" choice). Useful for the UI to show a loopback icon.</summary>
        public bool isLoopback;

        public DialogueChoice() { }

        public DialogueChoice(int index, string text, string[] tags = null, bool isLoopback = false)
        {
            this.index = index;
            this.text = text ?? string.Empty;
            this.tags = tags ?? Array.Empty<string>();
            this.isLoopback = isLoopback;
        }

        public override string ToString() => $"[{index}] {text}";
    }
}
