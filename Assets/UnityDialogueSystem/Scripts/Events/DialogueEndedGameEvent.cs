using System;
using UnityEngine;

namespace ZaiGames.DialogueSystem.Events
{
    /// <summary>
    /// Raised by <see cref="DialogueRunner"/> when the ink story has
    /// reached its end (no more content and no choices left).
    /// </summary>
    [CreateAssetMenu(menuName = "Dialogue/Events/Ended", order = 13)]
    public class DialogueEndedGameEvent : ScriptableObject
    {
        [TextArea] [SerializeField] private string description = "Raised when the story reaches its end.";

        private event Action _listeners;

        public void Raise() => _listeners?.Invoke();
        public void Register(Action listener) => _listeners += listener;
        public void Deregister(Action listener) => _listeners -= listener;
    }
}
