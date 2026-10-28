using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ZaiGames.DialogueSystem.UI
{
    /// <summary>
    /// Prefab component for a single choice button. The view instantiates
    /// one per <c>Ink.Runtime.Choice</c> and calls <see cref="Setup"/>
    /// to wire up the label, index, and click callback.
    /// </summary>
    public class ChoiceButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text label;
        [SerializeField] private TMP_Text indexLabel;

        private void Reset()
        {
            button = GetComponentInChildren<Button>();
            label = GetComponentInChildren<TMP_Text>();
        }

        /// <param name="index">0-based choice index (used to label "1", "2", ...).</param>
        /// <param name="text">Display text from <c>Choice.text</c>.</param>
        /// <param name="onClick">Callback invoked when the button is clicked.</param>
        public void Setup(int index, string text, Action onClick)
        {
            if (label != null)
                label.text = text ?? string.Empty;

            if (indexLabel != null)
                indexLabel.text = (index + 1).ToString();

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onClick?.Invoke());
            }
        }
    }
}
