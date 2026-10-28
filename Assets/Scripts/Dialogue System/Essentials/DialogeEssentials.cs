using UnityEngine;

namespace DialogueSystem.Essentials
{
    [System.Serializable]
    public class PlayerThoughts
    {
        [TextArea(3,10)]
        public string[] thoughts;
    }
}
