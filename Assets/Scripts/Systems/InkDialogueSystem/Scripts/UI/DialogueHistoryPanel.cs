// ---------------------------------------------------------------------------
// DialogueHistoryPanel.cs
// ---------------------------------------------------------------------------
// Scrollable list of every line shown so far this session. Subscribes to
// OnLineDisplayed and appends an entry to a vertical layout group inside
// a ScrollRect. Choice lines are rendered with a small "(choice)" prefix
// so the player can see at a glance where they branched.
//
// Performance: history is bounded by DialogueManager.historyCapacity
// (default 200). When the cap is hit, the oldest entry is removed from
// the bottom of the panel as well. For very long sessions consider
// virtualizing this list with a pool, but 200 plain TextMeshProUGUI
// entries is fine on most platforms.
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
    /// Append-only log of all dialogue lines shown this session.
    /// Toggleable from the main dialogue UI.
    /// </summary>
    public class DialogueHistoryPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;            // The panel root (toggled on/off).
        [SerializeField] private RectTransform contentParent; // Layout group parent.
        [SerializeField] private GameObject entryPrefab;     // TMP_Text prefab.
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private int maxVisibleEntries = 200;

        [Header("Styling")]
        [SerializeField] private Color speakerColor = new Color(0.42f, 0.85f, 1.0f);
        [SerializeField] private Color bodyColor = Color.white;
        [SerializeField] private Color choiceColor = new Color(1.0f, 0.78f, 0.35f);

        private readonly Queue<GameObject> _spawnedEntries = new Queue<GameObject>();

        private void OnEnable() => DialogueEvents.OnLineDisplayed += HandleLine;
        private void OnDisable() => DialogueEvents.OnLineDisplayed -= HandleLine;

        private void Start()
        {
            if (root != null) root.SetActive(false);
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

        private void HandleLine(DialogueLine line)
        {
            if (!isActiveAndEnabled) return;
            if (root == null || contentParent == null || entryPrefab == null) return;

            GameObject entry = Instantiate(entryPrefab, contentParent);
            if (entry.TryGetComponent(out TMP_Text text))
            {
                string prefix = string.IsNullOrEmpty(line.speaker)
                    ? ""
                    : $"<color=#{ColorUtility.ToHtmlStringRGBA(speakerColor)}>{line.speaker}:</color> ";
                string body = line.hasChoices
                    ? $"<color=#{ColorUtility.ToHtmlStringRGBA(choiceColor)}>{line.text}  (choice)</color>"
                    : $"<color=#{ColorUtility.ToHtmlStringRGBA(bodyColor)}>{line.text}</color>";
                text.text = prefix + body;
                text.richText = true;
            }
            _spawnedEntries.Enqueue(entry);

            while (_spawnedEntries.Count > maxVisibleEntries)
            {
                GameObject old = _spawnedEntries.Dequeue();
                if (old != null) Destroy(old);
            }

            // Snap scroll to bottom so the newest entry is visible.
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        /// <summary>Clear the visible panel (called when a new story is
        /// loaded). The DialogueManager's internal history is also cleared
        /// by StartStory; this just keeps the UI in sync.</summary>
        public void Clear()
        {
            foreach (var e in _spawnedEntries)
            {
                if (e != null) Destroy(e);
            }
            _spawnedEntries.Clear();
        }
    }
}
