using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using ZaiGames.DialogueSystem.Core;
using ZaiGames.DialogueSystem.Events;

namespace ZaiGames.DialogueSystem.UI
{
    /// <summary>
    /// uGUI + TextMeshPro view for the dialogue box. Subscribes to the
    /// four ScriptableObject dialogue events raised by
    /// <see cref="DialogueRunner"/> and renders:
    ///
    /// <list type="bullet">
    ///   <item>Speaker label (from <c>#speaker:...</c>)</item>
    ///   <item>Line text via <see cref="TypewriterEffect"/></item>
    ///   <item>Choice buttons instantiated from a prefab</item>
    ///   <item>A "press space to continue" prompt</item>
    /// </list>
    ///
    /// The view signals completion back to the runner via
    /// <see cref="DialogueRunner.NotifyTypewriterComplete"/>. Skip
    /// requests (advance key during typing) flow from runner → view via
    /// the <see cref="DialogueRunner.SkipRequested"/> C# event; the
    /// view calls <see cref="TypewriterEffect.Complete"/> to fast-
    /// forward the reveal.
    /// </summary>
    public class DialogueUIView : MonoBehaviour
    {
        [Header("Runner (for callbacks)")]
        [SerializeField] private DialogueRunner runner;

        [Header("UI bindings")]
        [Tooltip("Root GameObject that contains the whole dialogue box. Hidden when dialogue ends.")]
        [SerializeField] private GameObject root;

        [SerializeField] private TMP_Text speakerLabel;
        [SerializeField] private TypewriterEffect lineTypewriter;

        [Tooltip("Container RectTransform where choice buttons are instantiated.")]
        [SerializeField] private RectTransform choiceContainer;

        [SerializeField] private ChoiceButton choiceButtonPrefab;

        [Tooltip("Optional 'press space' indicator. Shown when phase is WaitingForContinue.")]
        [SerializeField] private GameObject continuePrompt;

        [Header("Subscribed events")]
        [SerializeField] private DialogueStartedGameEvent onStarted;
        [SerializeField] private DialogueLineGameEvent onLinePresented;
        [SerializeField] private DialogueChoicesGameEvent onChoicesPresented;
        [SerializeField] private DialogueEndedGameEvent onEnded;

        private readonly List<ChoiceButton> _choiceButtons = new();

        private void OnEnable()
        {
            if (runner != null) runner.SkipRequested += HandleSkipRequested;
            onStarted?.Register(HandleStarted);
            onLinePresented?.Register(HandleLinePresented);
            onChoicesPresented?.Register(HandleChoicesPresented);
            onEnded?.Register(HandleEnded);
        }

        private void OnDisable()
        {
            if (runner != null) runner.SkipRequested -= HandleSkipRequested;
            onStarted?.Deregister(HandleStarted);
            onLinePresented?.Deregister(HandleLinePresented);
            onChoicesPresented?.Deregister(HandleChoicesPresented);
            onEnded?.Deregister(HandleEnded);
        }

        // -----------------------------------------------------------------
        // Event handlers
        // -----------------------------------------------------------------

        private void HandleStarted(Story _)
        {
            if (root != null) root.SetActive(true);
            ClearChoices();
            HideContinuePrompt();
            if (speakerLabel != null) speakerLabel.text = string.Empty;
            if (lineTypewriter != null) lineTypewriter.Stop();
        }

        private async void HandleLinePresented(DialogueLine line)
        {
            HideContinuePrompt();
            ClearChoices();

            if (speakerLabel != null)
                speakerLabel.text = line.HasSpeaker ? line.Speaker : string.Empty;

            // Drive the typewriter; await its Awaitable. When it finishes
            // (naturally or via Complete()), signal the runner so it can
            // proceed to choices / waiting-for-continue.
            if (lineTypewriter != null)
            {
                await lineTypewriter.Play(line.Text);
            }
            else if (lineTextFallback != null)
            {
                lineTextFallback.text = line.Text;
            }

            runner?.NotifyTypewriterComplete();

            // Only show the continue prompt if there are no choices
            // waiting to be displayed (choices supersede continue).
            if (runner == null || runner.Phase != DialoguePhase.WaitingForChoice)
                ShowContinuePrompt();
        }

        private void HandleChoicesPresented(List<Choice> choices)
        {
            HideContinuePrompt();
            ClearChoices();

            if (choices == null) return;

            for (int i = 0; i < choices.Count; i++)
            {
                Choice choice = choices[i];
                if (choiceButtonPrefab == null || choiceContainer == null) continue;

                var btn = Instantiate(choiceButtonPrefab, choiceContainer);
                btn.Setup(i, choice.text, () => runner?.SelectChoice(i));
                _choiceButtons.Add(btn);
            }
        }

        private void HandleEnded()
        {
            ClearChoices();
            HideContinuePrompt();
            if (root != null) root.SetActive(false);
        }

        private void HandleSkipRequested()
        {
            // Runner forwarded a skip; complete the typewriter instantly.
            lineTypewriter?.Complete();
        }

        // -----------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------

        // Optional fallback if TypewriterEffect component is missing —
        // a plain TMP_Text we can write the full line to.
        [SerializeField] private TMP_Text lineTextFallback;

        private void ClearChoices()
        {
            for (int i = 0; i < _choiceButtons.Count; i++)
            {
                if (_choiceButtons[i] != null)
                    Destroy(_choiceButtons[i].gameObject);
            }
            _choiceButtons.Clear();
        }

        private void ShowContinuePrompt()
        {
            if (continuePrompt != null) continuePrompt.SetActive(true);
        }

        private void HideContinuePrompt()
        {
            if (continuePrompt != null) continuePrompt.SetActive(false);
        }
    }
}
