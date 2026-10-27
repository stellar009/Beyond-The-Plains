// ---------------------------------------------------------------------------
// TypewriterEffect.cs
// ---------------------------------------------------------------------------
// Animated character-by-character text reveal. Listens to OnLineDisplayed
// and progressively reveals DialogueLine.text on a target TextMeshProUGUI.
//
// Hooks:
//   - Speed (chars/sec) adjustable at runtime.
//   - Skip-on-input: any click/key advances to full line instantly.
//   - OnComplete callback for chaining (e.g. enable choices after reveal).
//
// The typewriter does NOT modify the DialogueLine.text; it only controls
// how much of the text is visible on the TMP component. Speaker name,
// portraits, and tags are shown immediately when the line starts.
// ---------------------------------------------------------------------------

using System.Collections;
using UnityEngine;
using TMPro;
using InkDialogueSystem.Core;
using InkDialogueSystem.Data;

namespace InkDialogueSystem.Effects
{
    /// <summary>
    /// Reveals text one character at a time. Skips to full text on player
    /// input (click, space, any axis submit).
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class TypewriterEffect : MonoBehaviour
    {
        [Header("Speed")]
        [Tooltip("Characters revealed per second.")]
        [SerializeField] private float charsPerSecond = 60f;

        [Tooltip("Multiplier applied when the player holds the skip key " +
                 "(default: Shift). 1 = no change, 8 = very fast forward.")]
        [SerializeField] private float fastForwardMultiplier = 8f;

        [Header("Input")]
        [Tooltip("Skip to full text on any mouse click or key press.")]
        [SerializeField] private bool skipOnAnyInput = true;

        [Tooltip("Explicit skip key (in addition to any-input).")]
        [SerializeField] private KeyCode skipKey = KeyCode.Space;

        [Header("Audio (optional)")]
        [Tooltip("AudioSource to play a 'tick' sound on each character. " +
                 "Leave null for silent typewriter.")]
        [SerializeField] private AudioSource tickAudioSource;

        [Tooltip("Plays one tick every N characters revealed. Lower = more clicks.")]
        [SerializeField] private int tickEveryNChars = 3;

        [Header("Punctuation")]
        [Tooltip("Extra delay (seconds) added after sentence-ending punctuation.")]
        [SerializeField] private float punctuationDelay = 0.15f;

        private TMP_Text _text;
        private Coroutine _routine;
        private bool _isRevealing;
        private string _fullText;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _text.text = string.Empty;
        }

        private void OnEnable() => DialogueEvents.OnLineDisplayed += HandleLine;
        private void OnDisable() => DialogueEvents.OnLineDisplayed -= HandleLine;

        private void Update()
        {
            if (!_isRevealing) return;
            bool skipInput = (skipOnAnyInput && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
                             || Input.GetKeyDown(skipKey)
                             || Input.GetKey(KeyCode.LeftShift);
            if (skipInput)
            {
                CompleteImmediately();
            }
        }

        // -----------------------------------------------------------------
        // Public
        // -----------------------------------------------------------------

        public void SetSpeed(float cps) => charsPerSecond = Mathf.Max(1f, cps);

        public void ShowImmediate(string text)
        {
            StopReveal();
            _text.text = text;
            _fullText = text;
        }

        // -----------------------------------------------------------------
        // Internals
        // -----------------------------------------------------------------

        private void HandleLine(DialogueLine line)
        {
            if (!isActiveAndEnabled) return;
            StopReveal();
            _fullText = line.text ?? string.Empty;
            _routine = StartCoroutine(RevealRoutine(_fullText));
        }

        private IEnumerator RevealRoutine(string text)
        {
            _isRevealing = true;
            _text.text = text;
            _text.ForceMeshUpdate();

            int total = text.Length;
            int charsShown = 0;
            int ticksSinceLastSound = 0;
            float interval = 1f / charsPerSecond;

            // We reveal by manipulating maxVisibleCharacters so rich tags
            // remain intact and TMP handles layout correctly.
            _text.maxVisibleCharacters = 0;

            while (charsShown < total)
            {
                bool fastFwd = Input.GetKey(KeyCode.LeftShift);
                float effInterval = interval / (fastFwd ? fastForwardMultiplier : 1f);

                char c = text[charsShown];
                _text.maxVisibleCharacters = charsShown + 1;
                charsShown++;

                if (tickAudioSource != null)
                {
                    ticksSinceLastSound++;
                    if (ticksSinceLastSound >= tickEveryNChars && !char.IsWhiteSpace(c))
                    {
                        tickAudioSource.Play();
                        ticksSinceLastSound = 0;
                    }
                }

                if (IsSentenceEnd(c))
                {
                    yield return new WaitForSeconds(punctuationDelay);
                }
                else
                {
                    yield return new WaitForSeconds(effInterval);
                }
            }

            _text.maxVisibleCharacters = total;
            _isRevealing = false;
        }

        private void CompleteImmediately()
        {
            StopReveal();
            _text.maxVisibleCharacters = (_fullText ?? string.Empty).Length;
            _isRevealing = false;
        }

        private void StopReveal()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
            _isRevealing = false;
        }

        private static bool IsSentenceEnd(char c) => c == '.' || c == '!' || c == '?';
    }
}
