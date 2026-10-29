using DialogueSystem.Core;
using UnityEngine;
using BeyondThePlains.InputSystem;

namespace DialogueSystem.Essentials
{
    public class DialogueTrigger : MonoBehaviour
    {
        private BoxCollider collider;
        private GameInputManager inputManager;
        [SerializeField] private DialogueManager dialogueManager;

        public PlayerThoughts thoughts;

        private void Start()
        {
            if(collider == null)
            {
                gameObject.AddComponent<BoxCollider>();
            }
            collider = GetComponent<BoxCollider>();
            collider.isTrigger = true;

            inputManager = FindAnyObjectByType<GameInputManager>();
        }

        public void StartThinking()
        {
            dialogueManager.StartDialogue(thoughts);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Player"))
            {
                inputManager.EnableDialogueInput(other.CompareTag("Player") && !dialogueManager.autoNextDialouge);
                StartThinking();
            }
        }
    }
}
