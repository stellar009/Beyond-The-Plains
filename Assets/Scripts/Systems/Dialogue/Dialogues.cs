using UnityEngine;

[System.Serializable]
public class Dialogues
{
    public string characterName;

    [TextArea(3, 10)]
    public string[] dialogue; 
}
