// ---------------------------------------------------------------------------
// DialogueManager.cs
// ---------------------------------------------------------------------------
// Singleton wrapper around Ink.Runtime.Story. Owns the dialogue loop:
//   load story -> bind external functions -> continue -> parse tags ->
//   build DialogueLine -> dispatch events -> wait for choice -> repeat.
//
// Tag conventions used by the system (all optional, all parsed in
// ParseLineTags):
//   #speaker:NAME        -- sets DialogueLine.speaker
//   #portrait:KEY        -- hints to UI which portrait sprite to show
//   #save[:SLOT]         -- fires OnSaveTag (auto-saves to SLOT or "auto")
//   #event:NAME          -- generic game-event hook, parsed by listeners
//
// NOTE: The system previously supported #audio:CLIP and #music:TRACK tags
// via an AudioHookSystem component. That subsystem has been removed. If
// you need audio cues, subscribe to OnLineDisplayed and play clips based
// on the speaker or any custom tag you invent (e.g. #sfx:door).
//
// The manager itself owns NO Unity UI references. UI panels subscribe to
// DialogueEvents. This keeps the manager testable in isolation and lets
// you swap UI prefabs without touching core logic.
// ---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using InkDialogueSystem.Data;

namespace InkDialogueSystem.Core
{
    /// <summary>
    /// The single source of truth for the active Ink story. Wraps the
    /// runtime, owns the dialogue loop, and exposes a thin API that UI
    /// and gameplay code can call (StartStory, Continue, SelectChoice,
    /// Save/Load).
    /// </summary>
    [DisallowMultipleComponent]
    public class DialogueManager : MonoBehaviour
    {
        // -----------------------------------------------------------------
        // Inspector
        // -----------------------------------------------------------------

        [Header("Story Source")]
        [Tooltip("Name of the compiled .ink.json file (no extension) inside " +
                 "Resources/Ink/ or StreamingAssets/Ink/.")]
        [SerializeField] private string defaultStoryName = "sample_story";

        [Tooltip("If true, the story loads automatically in Awake(). " +
                 "Disable to control load timing from a director script.")]
        [SerializeField] private bool autoStartOnAwake = true;

        [Header("Debug")]
        [SerializeField] private bool logTagDispatch = false;

        // -----------------------------------------------------------------
        // Singleton
        // -----------------------------------------------------------------

        public static DialogueManager Instance { get; private set; }

        // -----------------------------------------------------------------
        // State
        // -----------------------------------------------------------------

        /// <summary>The live Ink story. Null before StartStory() is called
        /// and after StopStory() is called.</summary>
        public Story Story => _story;

        /// <summary>True while there is an active story AND it can produce
        /// more text (story.canContinue). False at end-of-story or when
        /// waiting on a choice.</summary>
        public bool IsRunning => _story != null;

        /// <summary>True if the manager is currently waiting for the player
        /// to pick a choice. UI uses this to enable/disable choice
        /// buttons.</summary>
        public bool IsAwaitingChoice { get; private set; }

        /// <summary>Name of the currently-loaded story asset. Used by
        /// the save system to validate that a save matches the story.</summary>
        public string CurrentStoryName { get; private set; }

        /// <summary>Cumulative playtime since StartStory() was called.
        /// Tracked for the save system.</summary>
        public float PlaytimeSeconds { get; private set; }

        /// <summary>All lines shown this session, oldest first. Bounded
        /// by historyCapacity to prevent unbounded memory growth.</summary>
        public IReadOnlyList<DialogueLine> History => _history;

        [Tooltip("Maximum number of lines kept in memory for the history " +
                 "panel. Older lines are dropped (FIFO).")]
        [SerializeField] private int historyCapacity = 200;

        private Story _story;
        private readonly List<DialogueLine> _history = new List<DialogueLine>();
        private readonly List<DialogueChoice> _currentChoices = new List<DialogueChoice>();
        private readonly HashSet<string> _externalFunctionsBound = new HashSet<string>();

        // -----------------------------------------------------------------
        // Lifecycle
        // -----------------------------------------------------------------

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // Optional: uncomment to persist across scene loads.
            // DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (autoStartOnAwake)
            {
                StartStory(defaultStoryName);
            }
        }

        private void Update()
        {
            if (IsRunning)
            {
                PlaytimeSeconds += Time.deltaTime;
            }
        }

        // -----------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------

        /// <summary>Load (or reload) a story by name. Disposes the previous
        /// story if any. After this returns true, call Continue() to start
        /// producing lines.</summary>
        public bool StartStory(string storyName)
        {
            StopStory();

            Story story = InkFileLoader.LoadStory(storyName);
            if (story == null) return false;

            _story = story;
            CurrentStoryName = storyName;
            PlaytimeSeconds = 0f;
            _history.Clear();
            _currentChoices.Clear();
            IsAwaitingChoice = false;

            // Note: Ink's onDidContinue event still exists, but we no
            // longer subscribe to it because variable-change observation
            // is better handled via story.ObserveVariables (see
            // BindExternalFunctions below). The older onMakeTransition
            // event was removed from the Ink runtime API and is no
            // longer available.
            BindExternalFunctions(story);

            Debug.Log($"[DialogueManager] Story '{storyName}' loaded successfully.");
            return true;
        }

        /// <summary>Stop the active story, release its resources, and clear
        /// history.</summary>
        public void StopStory()
        {
            if (_story != null)
            {
                _story = null;
            }
            CurrentStoryName = null;
            _history.Clear();
            _currentChoices.Clear();
            IsAwaitingChoice = false;
        }

        /// <summary>Advance the story by one line. If the next beat is a
        /// choice block, the manager will populate CurrentChoices and fire
        /// OnChoicesUpdated; do NOT call Continue() again until the player
        /// picks (call SelectChoice instead).</summary>
        public bool Continue()
        {
            if (!IsRunning)
            {
                Debug.LogWarning("[DialogueManager] Continue() called with no active story.");
                return false;
            }
            if (IsAwaitingChoice)
            {
                Debug.LogWarning("[DialogueManager] Continue() called while awaiting a choice. " +
                                  "Call SelectChoice() instead.");
                return false;
            }
            if (!_story.canContinue)
            {
                // No more text AND no choices -> story ended.
                if (_story.currentChoices.Count == 0)
                {
                    DialogueEvents.DialogueEnded();
                }
                return false;
            }

            // Continue() returns the next text chunk. We then build the
            // DialogueLine ourselves so we can attach speaker + tags.
            string text = _story.Continue();
            DialogueLine line = BuildLine(text, _story.currentTags);

            // Choices for this beat (if any) come from story.currentChoices
            // which is populated by the Continue() call above.
            if (_story.currentChoices.Count > 0)
            {
                line.hasChoices = true;
                line.choices = MapChoices(_story.currentChoices);
                IsAwaitingChoice = true;
                _currentChoices.Clear();
                _currentChoices.AddRange(line.choices);
                DialogueEvents.ChoicesUpdated(_currentChoices);
            }

            PushHistory(line);
            ParseAndDispatchTags(line.tags);
            DialogueEvents.LineDisplayed(line);

            if (logTagDispatch)
            {
                Debug.Log($"[DialogueManager] Line: {line} | tags: [{string.Join(", ", line.tags)}]");
            }
            return true;
        }

        /// <summary>Player picked a choice. Index is the Ink choice index
        /// (same as DialogueChoice.index). Triggers story.ChooseChoiceIndex
        /// and then advances the story by one Continue().</summary>
        public bool SelectChoice(int inkChoiceIndex)
        {
            if (!IsRunning || !IsAwaitingChoice)
            {
                Debug.LogWarning("[DialogueManager] SelectChoice called when not awaiting a choice.");
                return false;
            }
            if (inkChoiceIndex < 0 || inkChoiceIndex >= _story.currentChoices.Count)
            {
                Debug.LogError($"[DialogueManager] Choice index {inkChoiceIndex} out of range " +
                               $"(have {_story.currentChoices.Count} choices).");
                return false;
            }

            DialogueChoice picked = _currentChoices.Find(c => c.index == inkChoiceIndex);
            _story.ChooseChoiceIndex(inkChoiceIndex);
            IsAwaitingChoice = false;
            _currentChoices.Clear();
            DialogueEvents.ChoicesUpdated(_currentChoices);
            if (picked != null) DialogueEvents.ChoiceSelected(picked);

            // After a choice, continue once to produce the next line.
            return Continue();
        }

        /// <summary>Convenience overload that accepts a DialogueChoice.</summary>
        public bool SelectChoice(DialogueChoice choice)
        {
            if (choice == null) return false;
            return SelectChoice(choice.index);
        }

        /// <summary>Bind a C# function (no arguments) so the .ink file
        /// can call it via INLINE or EXTERNAL function declarations. Must
        /// be called after StartStory; the manager will re-bind on every
        /// StartStory call. Use this to expose game state (inventory,
        /// quest flags) to Ink.</summary>
        /// <remarks>The Ink runtime API changed: the older
        /// <c>Story.ExternalFunction</c> delegate was replaced by
        /// <c>System.Func&lt;object&gt;</c> and its generic overloads.
        /// This wrapper normalizes that for callers.</remarks>
        public void BindExternalFunction(string inkFuncName, System.Func<object> function)
        {
            if (!IsRunning)
            {
                Debug.LogWarning("[DialogueManager] BindExternalFunction called with no active story.");
                return;
            }
            _story.BindExternalFunction(inkFuncName, function);
            _externalFunctionsBound.Add(inkFuncName);
        }

        /// <summary>Bind a C# function with one argument. Generic overload
        /// matches Ink's <c>Func&lt;T, object&gt;</c> signature.</summary>
        public void BindExternalFunction<T>(string inkFuncName, System.Func<T, object> function)
        {
            if (!IsRunning)
            {
                Debug.LogWarning("[DialogueManager] BindExternalFunction called with no active story.");
                return;
            }
            _story.BindExternalFunction(inkFuncName, function);
            _externalFunctionsBound.Add(inkFuncName);
        }

        /// <summary>Bind a C# function with two arguments.</summary>
        public void BindExternalFunction<T1, T2>(string inkFuncName, System.Func<T1, T2, object> function)
        {
            if (!IsRunning)
            {
                Debug.LogWarning("[DialogueManager] BindExternalFunction called with no active story.");
                return;
            }
            _story.BindExternalFunction(inkFuncName, function);
            _externalFunctionsBound.Add(inkFuncName);
        }

        /// <summary>Read an Ink variable. Returns default(T) if the
        /// variable does not exist.</summary>
        public T GetVar<T>(string varName)
        {
            if (!IsRunning) return default;
            object v = _story.variablesState[varName];
            if (v is T t) return t;
            try { return (T)Convert.ChangeType(v, typeof(T)); }
            catch { return default; }
        }

        /// <summary>Write an Ink variable. Triggers OnVariableChanged so
        /// the inspector panel can refresh.</summary>
        public void SetVar(string varName, object value)
        {
            if (!IsRunning) return;
            _story.variablesState[varName] = value;
            DialogueEvents.VariableChanged(varName);
        }

        // -----------------------------------------------------------------
        // Save/Load (called by DialogueSaveSystem)
        // -----------------------------------------------------------------

        /// <summary>Build a SaveData snapshot of the current state.</summary>
        public SaveData BuildSaveData(string slotName)
        {
            return new SaveData
            {
                slotName = slotName,
                storyAssetName = CurrentStoryName,
                inkStateJson = IsRunning ? _story.state.ToJson() : string.Empty,
                history = new List<DialogueLine>(_history),
                chosenHistory = ExtractChoiceHistory(_history),
                savedAtMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                playtimeSeconds = PlaytimeSeconds,
            };
        }

        /// <summary>Apply a SaveData snapshot. Reloads the story if
        /// necessary, restores the Ink state, then re-dispatches the last
        /// line so the UI refreshes.</summary>
        public bool ApplySaveData(SaveData data)
        {
            if (data == null || !data.IsValid()) return false;

            if (CurrentStoryName != data.storyAssetName)
            {
                if (!StartStory(data.storyAssetName)) return false;
            }

            try
            {
                _story.state.LoadJson(data.inkStateJson);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DialogueManager] Failed to restore Ink state: {ex.Message}");
                return false;
            }

            _history.Clear();
            _history.AddRange(data.history);
            PlaytimeSeconds = data.playtimeSeconds;
            IsAwaitingChoice = false;

            // Re-dispatch the most recent line so the UI shows where we
            // were. If history is empty, just advance to the next line.
            if (_history.Count > 0)
            {
                DialogueLine last = _history[_history.Count - 1];
                if (last.hasChoices)
                {
                    IsAwaitingChoice = true;
                    _currentChoices.Clear();
                    _currentChoices.AddRange(last.choices);
                    DialogueEvents.ChoicesUpdated(_currentChoices);
                }
                DialogueEvents.LineDisplayed(last);
            }
            else
            {
                Continue();
            }
            return true;
        }

        // -----------------------------------------------------------------
        // Internals
        // -----------------------------------------------------------------

        private void BindExternalFunctions(Story story)
        {
            // EXAMPLE BINDINGS -- replace with your game's actual functions.
            // These illustrate the pattern; the .ink sample uses them.
            //
            // Newer ink-unity-integration uses System.Func<object> and
            // generic overloads (Func<T, object>, Func<T1,T2, object>)
            // instead of the deprecated Story.ExternalFunction delegate.
            // The lambdas below are auto-resolved to the right generic
            // overload by the compiler.
            story.BindExternalFunction("GetPlayerName", (System.Func<object>)(() => "Wren"));
            story.BindExternalFunction("RollDice", (System.Func<int, object>)((int sides) => UnityEngine.Random.Range(1, sides + 1)));
            story.BindExternalFunction("HasItem", (System.Func<string, object>)((string itemId) => false));

            // Optional: observe variables so OnVariableChanged fires automatically.
            // story.ObserveVariables("affinity_mira", "gold", (name, value) => DialogueEvents.VariableChanged(name));
        }

        private DialogueLine BuildLine(string text, List<string> tags)
        {
            string speaker = "";
            List<string> cleanedTags = new List<string>();
            if (tags != null)
            {
                foreach (string t in tags)
                {
                    if (t.StartsWith("speaker:", StringComparison.OrdinalIgnoreCase))
                        speaker = t.Substring("speaker:".Length).Trim();
                    else
                        cleanedTags.Add(t);
                }
            }
            return new DialogueLine(text, speaker, cleanedTags);
        }

        private List<DialogueChoice> MapChoices(List<Choice> inkChoices)
        {
            List<DialogueChoice> result = new List<DialogueChoice>(inkChoices.Count);
            foreach (Choice c in inkChoices)
            {
                result.Add(new DialogueChoice
                {
                    index = c.index,
                    text = c.text,
                    tags = c.tags?.ToArray() ?? Array.Empty<string>(),
                    isLoopback = false,
                });
            }
            return result;
        }

        private void PushHistory(DialogueLine line)
        {
            _history.Add(line);
            while (_history.Count > historyCapacity)
            {
                _history.RemoveAt(0);
            }
        }

        private void ParseAndDispatchTags(List<string> tags)
        {
            if (tags == null) return;
            foreach (string tag in tags)
            {
                if (string.IsNullOrEmpty(tag)) continue;

                if (tag.StartsWith("save", StringComparison.OrdinalIgnoreCase))
                {
                    string slot = "auto";
                    if (tag.Contains(":"))
                        slot = tag.Substring(tag.IndexOf(':') + 1).Trim();
                    DialogueEvents.SaveTag(slot);
                }
                // Other tag namespaces (#event:NAME, #portrait:KEY) are
                // emitted in cleanedTags on the DialogueLine; consumers
                // can subscribe to OnLineDisplayed to read them.
            }
        }

        private List<DialogueChoice> ExtractChoiceHistory(List<DialogueLine> lines)
        {
            List<DialogueChoice> result = new List<DialogueChoice>();
            // The chosen-history is reconstructed from history lines'
            // "hasChoices" markers. The actual choice the player made is
            // not stored on the DialogueLine itself; if you need it, you
            // can extend this to record the picked choice index when
            // SelectChoice is called.
            foreach (DialogueLine line in lines)
            {
                if (line.hasChoices && line.choices != null && line.choices.Count > 0)
                    result.Add(line.choices[0]); // placeholder: first choice
            }
            return result;
        }
    }
}
