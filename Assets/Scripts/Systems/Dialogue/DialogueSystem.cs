using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI dialogueBox;
    [Range(0.01f, 0.1f)] public float delay = 0.05f;

    private Queue<string> m_Sentences = new Queue<string>();

    private GameInputManager m_Inputs;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_Inputs = FindObjectOfType<GameInputManager>();

        EnableDialogueObject(characterName, dialogueBox, false);
    }

    public void StartDialogue(Dialogues dialogue)
    {
        EnableDialogueObject(characterName, dialogueBox, true);

        characterName.text = dialogue.characterName;

        m_Sentences.Clear();

        foreach(string sentence in dialogue.dialogue)
        {
            m_Sentences.Enqueue(sentence);
        }

        NextSenetence();
        m_Inputs.TalkingStarted(true);
    }

    public void NextSenetence()
    {
        if(m_Sentences.Count == 0)
        {
            m_Inputs.DialogueCompleted(true);

            EnableDialogueObject(characterName, dialogueBox, false);

            return;
        }

        m_Inputs.DialogueCompleted(false);
        string sentence = m_Sentences.Dequeue();

        StartCoroutine(TypeWriter(sentence, dialogueBox, delay));
    }

    IEnumerator TypeWriter(string sentence, TextMeshProUGUI dialogueBoxTMP, float delayBetweenEachCharacter)
    {
        dialogueBoxTMP.text = sentence;
        dialogueBoxTMP.maxVisibleCharacters = 0;
        

        while(dialogueBoxTMP.maxVisibleCharacters < sentence.Length)
        {
            dialogueBoxTMP.maxVisibleCharacters++;

            yield return new WaitForSeconds(delayBetweenEachCharacter);
        }
    }

    void EnableDialogueObject(TextMeshProUGUI characterName, TextMeshProUGUI dialogueBox, bool enable)
    {
        characterName.gameObject.SetActive(enable);
        dialogueBox.gameObject.SetActive(enable);

        m_Inputs.TalkingStarted(false);
        m_Inputs.DialogueCompleted(false);
    }
}
