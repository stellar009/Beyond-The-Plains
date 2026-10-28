using System;
using UnityEngine;
using Ink.Runtime;

namespace ZaiGames.DialogueSystem.Events
{
    /// <summary>
    /// Raised by <see cref="DialogueRunner"/> when a new Story instance
    /// has been created and the dialogue is about to begin. The payload
    /// is the live <see cref="Story"/> object, which lets subscribers
    /// observe variables, bind external functions, etc.
    /// </summary>
    [CreateAssetMenu(menuName = "Dialogue/Events/Started", order = 12)]
    public class DialogueStartedGameEvent : ScriptableObject
    {
        [TextArea] [SerializeField] private string description = "Raised when a story starts. Payload is the live Story.";

        private event Action<Story> _listeners;

        public void Raise(Story story) => _listeners?.Invoke(story);
        public void Register(Action<Story> listener) => _listeners += listener;
        public void Deregister(Action<Story> listener) => _listeners -= listener;
    }
}
