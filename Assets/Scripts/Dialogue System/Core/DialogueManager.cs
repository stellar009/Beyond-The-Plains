using DialogueSystem.Essentials;
using DialogueSystem.UI;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.Core
{
    public class DialogueManager : MonoBehaviour
    {
        private Queue<string> sentences;
        private DialogueSystemUI m_DialogueSystemUI;

        public bool autoNextDialouge = true;
        [Range (0.5f, 1f)] public float delayToNextDialogue = 0.5f;

        private void Start()
        {
            sentences = new Queue<string>();

            m_DialogueSystemUI = FindAnyObjectByType<DialogueSystemUI>();
        }

        public void StartDialogue(PlayerThoughts thought)
        {
            m_DialogueSystemUI.StartThinking();

            sentences.Clear();

            foreach (string sentence in thought.thoughts)
            {
                sentences.Enqueue(sentence);
            }

            if(autoNextDialouge)
            {
                InvokeRepeating(nameof(DisplayNextSentence), delayToNextDialogue, delayToNextDialogue * 3);
            }
            else
            {
                DisplayNextSentence();
            }        
        }

        public void DisplayNextSentence()
        {
            if (sentences.Count == 0)
            {
                SentenceEnd();
                return;
            }

            string thought = sentences.Dequeue();
            m_DialogueSystemUI.UpdateThoughts(thought);
        }

        void SentenceEnd()
        {
        CancelInvoke();
            m_DialogueSystemUI.CompleteThinking();
            sentences.Clear();
        }
    }
}
