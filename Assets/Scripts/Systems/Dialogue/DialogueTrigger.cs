using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogues dialogue;


    public void TriggerDialogue()
    {
        FindObjectOfType<DialogueSystem>().StartDialogue(dialogue);
    }
}
