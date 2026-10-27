// ---------------------------------------------------------------------------
// DialogueUIController.cs
// ---------------------------------------------------------------------------
// Orchestrates the dialogue panel. Owns references to the TextMeshPro
// text field, the speaker label, the choice container, and the buttons
// (continue, auto-advance, etc.). Subscribes to DialogueEvents to drive
// the visual state.
//
// Layout assumption (matches the prefab described in the README):
//
//   DialoguePanel (root, this GameObject)
//     +-- SpeakerLabel (TMP_Text)        -- optional, hide if empty
//     +-- LineText (TMP_Text + TypewriterEffect)
//     +-- ChoicesContainer (RectTransform)
//     |     +-- ChoiceButtonPrefab (will be instantiated N times)
//     +-- ContinueButton (auto-hides when choices are present)
//     +-- HistoryToggle / InspectorToggle (open side panels)
//
// The controller does NOT know about Ink directly; it only consumes
// DialogueLine / DialogueChoice POPO objects and calls DialogueManager
// for input. This keeps it swappable for different UI toolkits (UI
// Toolkit, uGUI, custom canvas).
// ---------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InkDialogueSystem.Core;
using InkDialogueSystem.Data;

namespace InkDialogueSystem.UI
{
    /// <summary>
    /// Wires the dialogue UI to DialogueEvents. Handles line display,
    /// choice spawning, continue-button logic, and panel toggling.
    /// </summary>
    public class DialogueUIController : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private TMP_Text speakerLabel;
        [SerializeField] private TMP_Text lineText;

        [Header("Choices")]
        [SerializeField] private RectTransform choicesContainer;
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private int maxChoicesVisible = 6;

        [Header("Buttons")]
        [SerializeField] private GameObject continueButtonRoot;
        [SerializeField] private Button continueButton;
        [SerializeField] private GameObject choicePromptRoot;

        [Header("Side Panels")]
        [SerializeField] private DialogueHistoryPanel historyPanel;
        [SerializeField] private VariableInspectorPanel variableInspectorPanel;
        [SerializeField] private Button historyToggleButton;
        [SerializeField] private Button inspectorToggleButton;

        [Header("Cosmetic")]
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private float panelFadeDuration = 0.2f;
        [SerializeField] private GameObject portraitRoot;
        [SerializeField] private Image portraitImage;

        private readonly List<ChoiceButton> _spawnedChoiceButtons = new List<ChoiceButton>();

        private void OnEnable()
        {
            DialogueEvents.OnLineDisplayed += HandleLine;
            DialogueEvents.OnChoicesUpdated += HandleChoices;
            DialogueEvents.OnDialogueEnded += HandleDialogueEnded;

            if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
            if (historyToggleButton != null) historyToggleButton.onClick.AddListener(OnHistoryToggle);
            if (inspectorToggleButton != null) inspectorToggleButton.onClick.AddListener(OnInspectorToggle);
        }

        private void OnDisable()
        {
            DialogueEvents.OnLineDisplayed -= HandleLine;
            DialogueEvents.OnChoicesUpdated -= HandleChoices;
            DialogueEvents.OnDialogueEnded -= HandleDialogueEnded;

            if (continueButton != null) continueButton.onClick.RemoveListener(OnContinueClicked);
            if (historyToggleButton != null) historyToggleButton.onClick.RemoveListener(OnHistoryToggle);
            if (inspectorToggleButton != null) inspectorToggleButton.onClick.RemoveListener(OnInspectorToggle);
        }

        private void Start()
        {
            // Initial visual state: hide choices, show continue.
            SetChoicesVisible(false);
            if (continueButtonRoot != null) continueButtonRoot.SetActive(true);
            if (choicePromptRoot != null) choicePromptRoot.SetActive(false);
        }

        // -----------------------------------------------------------------
        // Event handlers
        // -----------------------------------------------------------------

        private void HandleLine(DialogueLine line)
        {
            // Speaker label: hide if no speaker, show otherwise.
            if (speakerLabel != null)
            {
                bool hasSpeaker = !string.IsNullOrEmpty(line.speaker);
                speakerLabel.gameObject.SetActive(hasSpeaker);
                speakerLabel.text = hasSpeaker ? line.speaker : "";
            }

            // The TypewriterEffect component (assumed to live on lineText's
            // GameObject) will animate the text. We just set the full text
            // here so it can be skipped to.
            if (lineText != null)
            {
                lineText.text = line.text;
                lineText.maxVisibleCharacters = line.text.Length;
                // The TypewriterEffect will overwrite maxVisibleCharacters
                // when it starts its reveal routine.
            }

            // Portrait hint via #portrait:KEY tag.
            ApplyPortraitTag(line.tags);

            // Choices container visibility handled in HandleChoices.
        }

        private void HandleChoices(List<DialogueChoice> choices)
        {
            ClearSpawnedChoices();
            bool hasChoices = choices != null && choices.Count > 0;
            SetChoicesVisible(hasChoices);

            if (continueButtonRoot != null) continueButtonRoot.SetActive(!hasChoices);
            if (choicePromptRoot != null) choicePromptRoot.SetActive(hasChoices);

            if (!hasChoices) return;

            int count = Mathf.Min(choices.Count, maxChoicesVisible);
            for (int i = 0; i < count; i++)
            {
                ChoiceButton btn = SpawnChoiceButton(choices[i]);
                _spawnedChoiceButtons.Add(btn);
            }
        }

        private void HandleDialogueEnded()
        {
            if (continueButtonRoot != null) continueButtonRoot.SetActive(false);
            if (choicePromptRoot != null) choicePromptRoot.SetActive(false);
            ClearSpawnedChoices();
            if (lineText != null) lineText.text = "[ End of story ]";
            if (speakerLabel != null) speakerLabel.gameObject.SetActive(false);
        }

        // -----------------------------------------------------------------
        // Button callbacks
        // -----------------------------------------------------------------

        private void OnContinueClicked()
        {
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.Continue();
        }

        private void OnHistoryToggle()
        {
            if (historyPanel != null) historyPanel.Toggle();
        }

        private void OnInspectorToggle()
        {
            if (variableInspectorPanel != null) variableInspectorPanel.Toggle();
        }

        // -----------------------------------------------------------------
        // Internals
        // -----------------------------------------------------------------

        private ChoiceButton SpawnChoiceButton(DialogueChoice choice)
        {
            GameObject go = Instantiate(choiceButtonPrefab, choicesContainer);
            ChoiceButton btn = go.GetComponent<ChoiceButton>();
            if (btn == null)
            {
                Debug.LogError("[DialogueUIController] ChoiceButtonPrefab is missing the ChoiceButton component.");
                Destroy(go);
                return null;
            }
            btn.Initialize(choice, OnChoiceClicked);
            return btn;
        }

        private void OnChoiceClicked(DialogueChoice choice)
        {
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.SelectChoice(choice);
        }

        private void ClearSpawnedChoices()
        {
            foreach (var btn in _spawnedChoiceButtons)
            {
                if (btn != null && btn.gameObject != null) Destroy(btn.gameObject);
            }
            _spawnedChoiceButtons.Clear();
        }

        private void SetChoicesVisible(bool visible)
        {
            if (choicesContainer != null) choicesContainer.gameObject.SetActive(visible);
        }

        private void ApplyPortraitTag(List<string> tags)
        {
            if (tags == null || portraitRoot == null) return;
            foreach (string t in tags)
            {
                if (t.StartsWith("portrait:"))
                {
                    string key = t.Substring("portrait:".Length).Trim();
                    // Hook into your portrait sprite atlas here. For now
                    // we just toggle the root active and log the key.
                    portraitRoot.SetActive(true);
                    Debug.Log($"[DialogueUIController] Portrait requested: {key}");
                    return;
                }
            }
        }
    }
}
