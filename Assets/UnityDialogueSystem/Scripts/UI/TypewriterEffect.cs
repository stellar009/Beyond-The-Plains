using TMPro;
using UnityEngine;
using System.Threading;

namespace ZaiGames.DialogueSystem.UI
{
    /// <summary>
    /// Drives a character-by-character typewriter reveal on a
    /// <see cref="TextMeshProUGUI"/> using Unity 6 <see cref="Awaitable"/>.
    ///
    /// Lifecycle:
    /// - Caller awaits <see cref="Play"/>; the method returns when the
    ///   full text has been revealed (or when <see cref="Complete"/> is
    ///   called, which finishes instantly).
    /// - Optional per-letter SFX are played through the bound
    ///   <see cref="AudioSource"/> at a configurable cadence.
    ///
    /// The effect is awaitable-friendly: the <see cref="DialogueUIView"/>
    /// awaits <c>Play(line.Text)</c> and then signals the runner via
    /// <c>NotifyTypewriterComplete()</c>.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TypewriterEffect : MonoBehaviour
    {
        [Header("Speed")]
        [Tooltip("Characters revealed per second.")]
        [SerializeField] private float charsPerSecond = 60f;

        [Header("Audio (optional)")]
        [SerializeField] private AudioClip typeSfx;
        [SerializeField] private AudioSource audioSource;
        [Tooltip("Play the SFX every Nth character (1 = every char, 2 = every other, etc.).")]
        [SerializeField] private int sfxEveryNthChar = 2;

        private TextMeshProUGUI _text;
        private CancellationTokenSource _cts;
        private bool _completeRequested;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        /// <summary>
        /// Reveals <paramref name="fullText"/> one character at a time.
        /// Awaits completion. Safe to call again before the previous
        /// call finishes — the previous awaitable is cancelled.
        /// </summary>
        public async Awaitable Play(string fullText)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            // NOTE: do NOT reset _completeRequested here. If Complete()
            // was called before Play started (premature skip), we honor it.

            fullText ??= string.Empty;

            _text.text = fullText;
            _text.maxVisibleCharacters = 0;
            _text.ForceMeshUpdate();

            int total = fullText.Length;
            float delay = 1f / Mathf.Max(1f, charsPerSecond);

            try
            {
                for (int i = 0; i <= total; i++)
                {
                    if (_completeRequested)
                    {
                        _text.maxVisibleCharacters = total;
                        return;
                    }
                    if (_cts.IsCancellationRequested) return;

                    _text.maxVisibleCharacters = i;

                    if (audioSource != null && typeSfx != null
                        && i > 0 && i % sfxEveryNthChar == 0)
                    {
                        char prev = fullText[i - 1];
                        if (!char.IsWhiteSpace(prev))
                            audioSource.PlayOneShot(typeSfx);
                    }

                    if (i < total)
                        await Awaitable.WaitForSecondsAsync(delay, _cts.Token);
                }
                _text.maxVisibleCharacters = total;
            }
            finally
            {
                _completeRequested = false;
            }
        }

        /// <summary>
        /// Instantly completes the current reveal. Safe to call before
        /// <see cref="Play"/> starts (the flag is honored on next Play).
        /// </summary>
        public void Complete() => _completeRequested = true;

        /// <summary>Stops the reveal and cancels any in-flight awaitable.</summary>
        public void Stop()
        {
            _completeRequested = true;
            _cts?.Cancel();
        }

        private void OnDisable() => _cts?.Cancel();
    }
}
