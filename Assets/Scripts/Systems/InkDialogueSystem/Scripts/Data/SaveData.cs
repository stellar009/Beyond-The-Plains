// ---------------------------------------------------------------------------
// SaveData.cs
// ---------------------------------------------------------------------------
// The serialized form of a dialogue save slot. The DialogueSaveSystem
// builds this from the live Ink story state plus the dialogue history and
// writes it out as JSON via JsonUtility. Loading reverses the process.
// ---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using InkDialogueSystem.Data;

namespace InkDialogueSystem.Data
{
    /// <summary>
    /// Everything needed to restore a dialogue session: the Ink story's
    /// internal state (the Inklecompiled save token), the visited dialogue
    /// history, the active save slot name, and a few diagnostic fields.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>Human-readable name of the save slot (e.g. "auto",
        /// "slot_1", "checkpoint_3").</summary>
        public string slotName;

        /// <summary>The story's official save token. Returned by
        /// story.state.ToJson() and consumed by story.state.LoadJson().
        /// Contains callstack, variable values, visit counts, etc.</summary>
        public string inkStateJson;

        /// <summary>Path/Name of the .ink.json story file this save belongs
        /// to. Critical: prevents loading a save with the wrong story.</summary>
        public string storyAssetName;

        /// <summary>All dialogue lines shown so far this session, oldest
        /// first. Used by the history panel and for replay debugging.</summary>
        public List<DialogueLine> history = new List<DialogueLine>();

        /// <summary>All choices the player has selected, oldest first.
        /// Subset of history but filtered for replay clarity.</summary>
        public List<DialogueChoice> chosenHistory = new List<DialogueChoice>();

        /// <summary>Wall-clock timestamp of when this save was created,
        /// Unix-epoch milliseconds.</summary>
        public long savedAtMs;

        /// <summary>Cumulative playtime at the moment of save, in seconds.
        /// Tracked by DialogueManager so saves remain comparable across
        /// sessions.</summary>
        public float playtimeSeconds;

        /// <summary>Optional game-specific payload. Implementations can
        /// stash inventory, quest state, character affection, etc. here
        /// without modifying the core save schema.</summary>
        public string customPayloadJson;

        public SaveData()
        {
            slotName = "auto";
            inkStateJson = string.Empty;
            storyAssetName = string.Empty;
            savedAtMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            playtimeSeconds = 0f;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(inkStateJson)
                && !string.IsNullOrEmpty(storyAssetName);
        }
    }
}
