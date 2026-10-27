// ---------------------------------------------------------------------------
// ChoiceButton.cs
// ---------------------------------------------------------------------------
// One choice button. Holds a reference to its DialogueChoice so it can
// hand it back to the controller when clicked. The prefab it lives on is
// expected to have a Button + a TMP_Text for the label.
// ---------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InkDialogueSystem.Data;

namespace InkDialogueSystem.UI
{
    /// <summary>
    /// Behavior on the choice-button prefab. Initialized by
    /// DialogueUIController with a DialogueChoice and a click callback.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ChoiceButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Button button;
        [SerializeField] private GameObject loopbackIndicator;

        private DialogueChoice _choice;
        private System.Action<DialogueChoice> _onClick;

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (label == null) label = GetComponentInChildren<TMP_Text>();
        }

        /// <summary>Wire up the button with its data and click handler.</summary>
        public void Initialize(DialogueChoice choice, System.Action<DialogueChoice> onClick)
        {
            _choice = choice;
            _onClick = onClick;
            if (label != null) label.text = choice.text;
            if (loopbackIndicator != null) loopbackIndicator.SetActive(choice.isLoopback);
        }

        // Hook this up in the Button's onClick event in the inspector, or
        // it will be auto-wired in Start().
        private void Start()
        {
            if (button != null) button.onClick.AddListener(HandleClick);
        }

        private void HandleClick()
        {
            _onClick?.Invoke(_choice);
        }
    }
}
