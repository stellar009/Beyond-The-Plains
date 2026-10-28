using UnityEngine;
using TMPro;

namespace DialogueSystem.UI
{
    public class DialogueSystemUI : MonoBehaviour
    {
        [SerializeField]private TextEffect _effect;

        [Header("Dialogue UI")]
        public TextMeshProUGUI dialogueTmp;

        private void Start()
        {
            if (!dialogueTmp) Debug.Log("No Dialogue TMP asigned in " + gameObject.name);
            dialogueTmp.gameObject.SetActive(false);
        }

        public void StartThinking()
        {
            dialogueTmp.gameObject.SetActive(true);
            dialogueTmp.text = string.Empty;
        }

        public void UpdateThoughts(string thought)
        {
            _effect.TypeWriter(dialogueTmp, thought);
        }

        public void CompleteThinking()
        {
            dialogueTmp.text = string.Empty;
            dialogueTmp.gameObject.SetActive(false);
        }
    }
}
