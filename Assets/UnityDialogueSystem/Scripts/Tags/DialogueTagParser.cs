using System.Collections.Generic;
using ZaiGames.DialogueSystem.Core;

namespace ZaiGames.DialogueSystem.Tags
{
    /// <summary>
    /// Parses the <c>#tag</c> list attached to the current ink line into a
    /// structured <see cref="DialogueLine"/>. Recognised standard tags:
    ///
    /// <list type="bullet">
    ///   <item><c>#speaker:Elena</c>      → <see cref="DialogueLine.Speaker"/></item>
    ///   <item><c>#portrait:happy</c>     → <see cref="DialogueLine.Portrait"/></item>
    ///   <item><c>#audio:vo_01</c>        → <see cref="DialogueLine.Audio"/></item>
    ///   <item><c>#mood:curious</c>       → <see cref="DialogueLine.Mood"/></item>
    /// </list>
    ///
    /// Any unrecognised <c>key:value</c> tag is stored in
    /// <see cref="DialogueLine.ExtraTags"/>. Bare tags (e.g. <c>#save</c>)
    /// are stored with value <c>"true"</c>.
    ///
    /// All keys are lower-cased for case-insensitive matching.
    /// </summary>
    public static class DialogueTagParser
    {
        public const string SpeakerKey  = "speaker";
        public const string PortraitKey = "portrait";
        public const string AudioKey    = "audio";
        public const string MoodKey     = "mood";

        public static DialogueLine Parse(string text, List<string> tags)
        {
            var line = new DialogueLine
            {
                Text = (text ?? string.Empty).Trim()
            };

            if (tags == null) return line;

            for (int i = 0; i < tags.Count; i++)
            {
                string raw = tags[i];
                if (string.IsNullOrEmpty(raw)) continue;

                string tag = raw.Trim();
                int colon = tag.IndexOf(':');

                string key, value;
                if (colon < 0)
                {
                    key = tag;
                    value = "true";
                }
                else
                {
                    key = tag.Substring(0, colon).Trim();
                    value = tag.Substring(colon + 1).Trim();
                }

                key = key.ToLowerInvariant();

                switch (key)
                {
                    case SpeakerKey:  line.Speaker  = value; break;
                    case PortraitKey: line.Portrait = value; break;
                    case AudioKey:    line.Audio    = value; break;
                    case MoodKey:     line.Mood     = value; break;
                    default:          line.ExtraTags[key] = value; break;
                }
            }

            return line;
        }
    }
}
