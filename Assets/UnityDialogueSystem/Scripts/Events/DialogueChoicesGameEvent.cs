using System;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

namespace ZaiGames.DialogueSystem.Events
{
    /// <summary>
    /// Raised by <see cref="DialogueRunner"/> when ink presents a list of
    /// choices that the player must select from before the story can advance.
    /// The payload is the live <c>List&lt;Choice&gt;</c> from
    /// <c>Story.currentChoices</c>.
    /// </summary>
    [CreateAssetMenu(menuName = "Dialogue/Events/Choices Presented", order = 11)]
    public class DialogueChoicesGameEvent : ScriptableObject
    {
        [TextArea] [SerializeField] private string description = "Raised when ink presents branching choices.";

        private event Action<List<Choice>> _listeners;

        public void Raise(List<Choice> choices)
        {
            _listeners?.Invoke(choices);
        }

        public void Register(Action<List<Choice>> listener) => _listeners += listener;
        public void Deregister(Action<List<Choice>> listener) => _listeners -= listener;
    }
}
