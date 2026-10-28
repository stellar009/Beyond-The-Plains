using System;
using UnityEngine;
using ZaiGames.DialogueSystem.Core;

namespace ZaiGames.DialogueSystem.Events
{
    /// <summary>
    /// Raised by <see cref="DialogueRunner"/> when a new line has been parsed
    /// and is ready to be displayed. UI views subscribe to this via
    /// <see cref="Register"/> / <see cref="Deregister"/>.
    ///
    /// Ryan Hipple-style ScriptableObject event: the asset is the channel,
    /// listeners are C# delegates, so any system can subscribe without
    /// a hard reference to the runner.
    /// </summary>
    [CreateAssetMenu(menuName = "Dialogue/Events/Line Presented", order = 10)]
    public class DialogueLineGameEvent : ScriptableObject
    {
        [TextArea] [SerializeField] private string description = "Raised when a new line is presented to the player.";

        private event Action<DialogueLine> _listeners;

        public void Raise(DialogueLine line)
        {
            if (line == null) return;
            _listeners?.Invoke(line);
        }

        public void Register(Action<DialogueLine> listener)
        {
            _listeners += listener;
        }

        public void Deregister(Action<DialogueLine> listener)
        {
            _listeners -= listener;
        }
    }
}
