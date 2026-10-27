// ---------------------------------------------------------------------------
// DialogueLine.cs
// ---------------------------------------------------------------------------
// A plain data object representing ONE line of dialogue that the Ink runtime
// produced. The DialogueManager builds these from the Ink story and pushes
// them to the UI. Keeping this as a separate POCO makes the rest of the
// system testable without a live Ink story and makes save/load trivial.
// ---------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace InkDialogueSystem.Data
{
    /// <summary>
    /// One rendered beat of dialogue: the text shown to the player, the
    /// optional speaker name (parsed from Ink tags), any tags attached to
    /// the line (used by the audio hook system, save points, etc.) and the
    /// list of choices the player can pick next (if any).
    /// </summary>
    [Serializable]
    public class DialogueLine
    {
        /// <summary>The plain-text content of the line, with Ink glue and
        /// inline expressions already evaluated by the runtime.</summary>
        public string text;

        /// <summary>Speaker name extracted from a #speaker:NAME tag, or
        /// empty if no speaker tag was present on the line.</summary>
        public string speaker;

        /// <summary>True if this line ends with a choice block. When true,
        /// <see cref="choices"/> is non-empty and the player must pick.</summary>
        public bool hasChoices;

        /// <summary>All tags attached to this line in the .ink file
        /// (e.g. #save, #portrait:angry, #event:quest_complete). Order preserved.</summary>
        public List<string> tags;

        /// <summary>Choices the player can pick. Empty when
        /// <see cref="hasChoices"/> is false.</summary>
        public List<DialogueChoice> choices;

        /// <summary>Wall-clock timestamp the line was generated, in seconds
        /// since Unix epoch. Used by the history panel for ordering and by
        /// the save system for replay diagnostics.</summary>
        public long timestampMs;

        public DialogueLine()
        {
            text = string.Empty;
            speaker = string.Empty;
            hasChoices = false;
            tags = new List<string>();
            choices = new List<DialogueChoice>();
            timestampMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public DialogueLine(string text, string speaker = "", List<string> tags = null)
            : this()
        {
            this.text = text;
            this.speaker = speaker ?? string.Empty;
            if (tags != null) this.tags = new List<string>(tags);
        }

        public override string ToString()
        {
            string prefix = string.IsNullOrEmpty(speaker) ? "" : speaker + ": ";
            return prefix + text;
        }
    }
}
