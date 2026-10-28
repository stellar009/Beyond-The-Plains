using System;
using System.Threading;
using UnityEngine;
using Ink.Runtime;
using ZaiGames.DialogueSystem.Events;
using ZaiGames.DialogueSystem.Tags;

namespace ZaiGames.DialogueSystem.Core
{
    /// <summary>
    /// Phase of the dialogue state machine. Drives what
    /// <see cref="DialogueInputPoller"/> and the UI view are allowed to do.
    /// </summary>
    public enum DialoguePhase
    {
        /// <summary>No story is loaded.</summary>
        Idle,
        /// <summary>A line is currently being revealed by the typewriter.</summary>
        Typing,
        /// <summary>Line fully shown; waiting for advance input to continue.</summary>
        WaitingForContinue,
        /// <summary>Choices are visible; waiting for the player to pick one.</summary>
        WaitingForChoice,
        /// <summary>Story has reached its end.</summary>
        Ended
    }

    /// <summary>
    /// Main driver of the dialogue system. Owns the live
    /// <see cref="Ink.Runtime.Story"/> instance, pumps it line-by-line,
    /// parses tags, raises ScriptableObject events for the view layer,
    /// and exposes a small surface for input + save/load integration.
    ///
    /// Architecture (Unity 6 Awaitable based):
    /// - <see cref="Begin"/> creates a Story from the bound
    ///   <see cref="DialogueStoryAsset"/> and starts the pump.
    /// - The pump reads <c>Continue()</c>, parses tags, raises
    ///   <see cref="onLinePresented"/>, then awaits the view's
    ///   <see cref="NotifyTypewriterComplete"/> callback.
    /// - If choices are present, raises <see cref="onChoicesPresented"/>
    ///   and stops — the caller must invoke <see cref="SelectChoice"/>.
    /// - Otherwise sets <see cref="Phase"/> to <c>WaitingForContinue</c>
    ///   and waits for <see cref="RequestAdvance"/>.
    ///
    /// The runner never references the UI view directly. The view is
    /// notified via ScriptableObject events and signals completion back
    /// via the <see cref="NotifyTypewriterComplete"/> callback. Skip
    /// requests are forwarded via the <see cref="SkipRequested"/> C#
    /// event so the view can complete its typewriter instantly.
    /// </summary>
    public class DialogueRunner : MonoBehaviour
    {
        [Header("Story source")]
        [SerializeField] private DialogueStoryAsset storyAsset;

        [Header("Events (raised by this runner)")]
        [SerializeField] private DialogueStartedGameEvent onStarted;
        [SerializeField] private DialogueLineGameEvent onLinePresented;
        [SerializeField] private DialogueChoicesGameEvent onChoicesPresented;
        [SerializeField] private DialogueEndedGameEvent onEnded;

        /// <summary>Live ink Story instance. Null when not running.</summary>
        public Story Story => _story;

        /// <summary>Current phase of the dialogue state machine.</summary>
        public DialoguePhase Phase { get; private set; } = DialoguePhase.Idle;

        /// <summary>Most recently presented line, or null.</summary>
        public DialogueLine CurrentLine { get; private set; }

        /// <summary>Raised whenever <see cref="Phase"/> changes.</summary>
        public event Action<DialoguePhase> PhaseChanged;

        /// <summary>
        /// Raised when input requests a typewriter skip. The UI view
        /// subscribes and calls <c>TypewriterEffect.Complete()</c>.
        /// </summary>
        public event Action SkipRequested;

        private Story _story;
        private CancellationTokenSource _cts;
        private bool _typewriterDone;

        // -----------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------

        /// <summary>
        /// Creates a fresh Story from the asset and begins the dialogue.
        /// If already running, the previous session is stopped first.
        /// </summary>
        public async Awaitable Begin()
        {
            if (storyAsset == null)
            {
                Debug.LogError($"[{nameof(DialogueRunner)}] No story asset assigned on '{gameObject.name}'.");
                return;
            }

            if (Phase != DialoguePhase.Idle)
                await Stop();

            _story = storyAsset.CreateStory();
            if (_story == null) return;

            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _typewriterDone = false;

            SetPhase(DialoguePhase.Typing);
            onStarted?.Raise(_story);

            await PumpAsync(_cts.Token);
        }

        /// <summary>
        /// Stops the dialogue, cancels any in-flight pump, and clears state.
        /// Safe to call from any phase.
        /// </summary>
        public async Awaitable Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _story = null;
            CurrentLine = null;
            _typewriterDone = false;
            SetPhase(DialoguePhase.Idle);
            await Awaitable.EndOfFrameAsync();
        }

        /// <summary>
        /// Called by the input poller when the player presses the
        /// advance key during <see cref="DialoguePhase.WaitingForContinue"/>.
        /// Continues to the next line.
        /// </summary>
        public void RequestAdvance()
        {
            if (Phase != DialoguePhase.WaitingForContinue) return;
            if (_story == null) return;
            _ = ContinueAsync();
        }

        /// <summary>
        /// Called by the input poller when the player presses the
        /// advance key during <see cref="DialoguePhase.Typing"/>.
        /// Forwards to the view via <see cref="SkipRequested"/>.
        /// </summary>
        public void RequestSkipTypewriter()
        {
            if (Phase != DialoguePhase.Typing) return;
            SkipRequested?.Invoke();
        }

        /// <summary>
        /// Called by either the input poller (digit keys) or the UI
        /// choice buttons. Validates the index, applies the choice to
        /// the story, and continues the pump.
        /// </summary>
        public void SelectChoice(int index)
        {
            if (Phase != DialoguePhase.WaitingForChoice) return;
            if (_story == null) return;
            if (index < 0 || index >= _story.currentChoices.Count)
            {
                Debug.LogWarning($"[{nameof(DialogueRunner)}] Choice index {index} out of range (0..{_story.currentChoices.Count - 1}).");
                return;
            }
            _story.ChooseChoiceIndex(index);
            _ = ContinueAsync();
        }

        /// <summary>
        /// Called by the UI view when its typewriter finishes (either
        /// naturally or after a skip). Unblocks the pump.
        /// </summary>
        public void NotifyTypewriterComplete()
        {
            _typewriterDone = true;
        }

        /// <summary>
        /// After <see cref="Save.DialogueSaveLoad.LoadInto"/> has loaded
        /// state into <see cref="Story"/>, call this to resume the pump
        /// from the saved position.
        /// </summary>
        public async Awaitable RestartFromLoadedState()
        {
            if (_story == null)
            {
                Debug.LogWarning($"[{nameof(DialogueRunner)}] RestartFromLoadedState called with no Story. Call Begin() first.");
                return;
            }
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _typewriterDone = false;
            SetPhase(DialoguePhase.Typing);
            await PumpAsync(_cts.Token);
        }

        // -----------------------------------------------------------------
        // Internal pump
        // -----------------------------------------------------------------

        private async Awaitable ContinueAsync()
        {
            if (_story == null) return;
            SetPhase(DialoguePhase.Typing);
            await PumpAsync(_cts?.Token ?? CancellationToken.None);
        }

        /// <summary>
        /// Core state machine. Reads one line from the story, displays
        /// it, then either presents choices, waits for continue, or
        /// ends. Re-entered via <see cref="ContinueAsync"/> after each
        /// input-driven step.
        /// </summary>
        private async Awaitable PumpAsync(CancellationToken ct)
        {
            while (_story != null && !ct.IsCancellationRequested)
            {
                if (_story.canContinue)
                {
                    // 1. Pull the next line of text from ink.
                    string text = _story.Continue();
                    var tags = _story.currentTags;

                    // 2. Parse tags into a structured DialogueLine.
                    CurrentLine = DialogueTagParser.Parse(text, tags);

                    // 3. Tell the view to start the typewriter.
                    SetPhase(DialoguePhase.Typing);
                    onLinePresented?.Raise(CurrentLine);

                    // 4. Wait for the view to finish (or be skipped).
                    _typewriterDone = false;
                    while (!_typewriterDone)
                    {
                        if (ct.IsCancellationRequested) return;
                        await Awaitable.EndOfFrameAsync(ct);
                    }

                    // 5. Decide what comes next.
                    if (_story.currentChoices.Count > 0)
                    {
                        SetPhase(DialoguePhase.WaitingForChoice);
                        onChoicesPresented?.Raise(_story.currentChoices);
                        return; // SelectChoice() will re-enter Pump via ContinueAsync()
                    }

                    if (!_story.canContinue)
                    {
                        SetPhase(DialoguePhase.Ended);
                        onEnded?.Raise();
                        return;
                    }

                    SetPhase(DialoguePhase.WaitingForContinue);
                    return; // RequestAdvance() will re-enter Pump via ContinueAsync()
                }

                if (_story.currentChoices.Count > 0)
                {
                    SetPhase(DialoguePhase.WaitingForChoice);
                    onChoicesPresented?.Raise(_story.currentChoices);
                    return;
                }

                // No content and no choices → end.
                SetPhase(DialoguePhase.Ended);
                onEnded?.Raise();
                return;
            }
        }

        private void SetPhase(DialoguePhase phase)
        {
            if (Phase == phase) return;
            Phase = phase;
            PhaseChanged?.Invoke(phase);
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}
