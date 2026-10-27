// ---------------------------------------------------------------------------
// VariableInspectorPanel.cs
// ---------------------------------------------------------------------------
// Debug panel that lists all Ink variables and their current values.
// Updates whenever OnVariableChanged fires, and also on a slow poll
// (once per second) to catch variables changed by Ink itself rather than
// by external code.
//
// This is primarily a developer tool. In shipping builds you typically
// hide it (or gate it behind a dev flag). The same data could be used
// to drive a "stats" screen in a more finished game.
// ---------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;
using InkDialogueSystem.Core;

namespace InkDialogueSystem.UI
{
    /// <summary>
    /// Live view of Ink runtime variables. Useful for debugging branching
    /// logic and verifying that choices affected variables as expected.
    /// </summary>
    public class VariableInspectorPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text outputLabel;
        [SerializeField] private Button closeButton;
        [SerializeField] private float refreshInterval = 1f;

        private float _timeSinceRefresh;
        private bool _dirty = true;

        private void OnEnable()
        {
            DialogueEvents.OnVariableChanged += HandleVariableChanged;
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
        }

        private void OnDisable()
        {
            DialogueEvents.OnVariableChanged -= HandleVariableChanged;
            if (closeButton != null) closeButton.onClick.RemoveListener(Hide);
        }

        private void Start()
        {
            if (root != null) root.SetActive(false);
        }

        private void Update()
        {
            if (root == null || !root.activeSelf) return;
            _timeSinceRefresh += Time.deltaTime;
            if (_timeSinceRefresh >= refreshInterval || _dirty)
            {
                _timeSinceRefresh = 0f;
                _dirty = false;
                Refresh();
            }
        }

        public void Toggle()
        {
            if (root != null) root.SetActive(!root.activeSelf);
        }

        public void Show()
        {
            if (root != null) root.SetActive(true);
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        // -----------------------------------------------------------------
        // Internals
        // -----------------------------------------------------------------

        private void HandleVariableChanged(string varName)
        {
            _dirty = true;
        }

        private void Refresh()
        {
            if (DialogueManager.Instance == null || DialogueManager.Instance.Story == null)
            {
                if (outputLabel != null) outputLabel.text = "<i>No active story.</i>";
                return;
            }

            Story story = DialogueManager.Instance.Story;
            StringBuilder sb = new StringBuilder();

            // variablesState iterates over all top-level variables defined
            // in the .ink file. Each entry is a {name -> value} pair.
            var variables = story.variablesState;
            sb.AppendLine("<b>Ink Variables</b>");
            sb.AppendLine();

            int count = 0;
            // JsonUtility-style enumeration: Ink's variablesState doesn't
            // expose a clean enumerator, so we use the reflection-free path
            // of probing a known list. For a more robust solution, register
            // your variables in story.ObserveVariables at startup.
            foreach (var name in _knownVariableNames)
            {
                object value = null;
                try { value = variables[name]; }
                catch { /* variable not defined in this story -- skip */ }
                if (value == null) continue;
                sb.AppendLine($"  <b>{name}</b> = {FormatValue(value)}");
                count++;
            }

            if (count == 0)
            {
                sb.AppendLine("  <i>No variables registered. Add names to " +
                              "_knownVariableNames or call story.ObserveVariables " +
                              "at startup to track them here.</i>");
            }

            sb.AppendLine();
            sb.AppendLine($"<b>Story state</b>");
            sb.AppendLine($"  canContinue: {story.canContinue}");
            sb.AppendLine($"  currentChoices: {story.currentChoices.Count}");

            if (outputLabel != null) outputLabel.text = sb.ToString();
        }

        private static string FormatValue(object value)
        {
            if (value == null) return "null";
            if (value is bool b) return b ? "true" : "false";
            if (value is string s) return $"\"{s}\"";
            return value.ToString();
        }

        /// <summary>Whitelist of variable names this panel will display.
        /// Populate it from your .ink file. For a fully automatic
        /// inspector, call DialogueManager.BindExternalFunction at
        /// startup and use story.ObserveVariables to track changes
        /// -- then list every observed variable here.</summary>
        [Tooltip("Names of the Ink variables to display. Must match the " +
                 "names declared at the top of your .ink file (VAR x = ...).")]
        [SerializeField] private List<string> _knownVariableNames = new List<string>
        {
            "affinity_mira",
            "affinity_kade",
            "gold",
            "met_stranger",
            "player_name",
        };
    }
}
