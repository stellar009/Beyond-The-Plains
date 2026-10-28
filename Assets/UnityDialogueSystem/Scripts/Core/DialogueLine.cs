using System.Collections.Generic;

namespace ZaiGames.DialogueSystem.Core
{
    /// <summary>
    /// A single parsed line of dialogue. Produced by
    /// <see cref="Tags.DialogueTagParser"/> from the raw ink
    /// <c>currentText</c> + <c>currentTags</c>.
    ///
    /// The standard fields (Speaker, Portrait, Audio, Mood) are populated
    /// from the recognised tag set; any other <c>key:value</c> tag is
    /// stored in <see cref="ExtraTags"/> so domain-specific systems can
    /// extend the vocabulary without modifying the parser.
    /// </summary>
    public sealed class DialogueLine
    {
        /// <summary>The text content of the line, trimmed of whitespace.</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>Speaker name from <c>#speaker:...</c>. Empty if not set.</summary>
        public string Speaker { get; set; } = string.Empty;

        /// <summary>Portrait key from <c>#portrait:...</c>. Empty if not set.</summary>
        public string Portrait { get; set; } = string.Empty;

        /// <summary>Audio cue from <c>#audio:...</c>. Empty if not set.</summary>
        public string Audio { get; set; } = string.Empty;

        /// <summary>Mood key from <c>#mood:...</c>. Empty if not set.</summary>
        public string Mood { get; set; } = string.Empty;

        /// <summary>
        /// Any other recognised tags. Key is lower-cased; value is the
        /// raw string after the colon (or "true" for bare tags).
        /// </summary>
        public readonly Dictionary<string, string> ExtraTags = new();

        public bool HasSpeaker  => !string.IsNullOrEmpty(Speaker);
        public bool HasPortrait => !string.IsNullOrEmpty(Portrait);
        public bool HasAudio    => !string.IsNullOrEmpty(Audio);
        public bool HasMood     => !string.IsNullOrEmpty(Mood);

        public bool TryGetExtra(string key, out string value)
            => ExtraTags.TryGetValue(key.ToLowerInvariant(), out value);

        public override string ToString()
        {
            string s = HasSpeaker ? $"[{Speaker}] " : "";
            return s + Text;
        }
    }
}
