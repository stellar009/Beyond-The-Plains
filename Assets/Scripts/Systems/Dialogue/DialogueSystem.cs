using System.Collections.Generic;
using UnityEngine;

public class DialogueSystem : MonoBehaviour
{
    private Queue<string> m_Sentences = new Queue<string>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public void StartDialogue(Dialogues dialogue)
    {
        m_Sentences.Clear();

        foreach(string sentence in dialogue.dialogue)
        {
            m_Sentences.Enqueue(sentence);
        }

        NextSenetence();
    }

    public void NextSenetence()
    {
        if(m_Sentences.Count == 0)
        {
            return;
        }

        string sentence = m_Sentences.Dequeue();

        Debug.Log(sentence);
    }
}
