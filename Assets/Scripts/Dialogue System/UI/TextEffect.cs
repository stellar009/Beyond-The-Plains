using System.Collections;
using TMPro;
using UnityEngine;
using BeyondThePlains.InputSystem;


namespace DialogueSystem.UI
{
    public class TextEffect : MonoBehaviour
    {
        private GameInputManager _InputManager;

        [Header("Settings")]
        [SerializeField]
        [Range(0.01f, 0.1f)]
        private float charDelay = 0.03f;

        private void Start()
        {
            _InputManager = FindAnyObjectByType<GameInputManager>();
        }

        #region TypeWriter
        public void TypeWriter(TextMeshProUGUI thoughtTmp, string thoughts)
            => StartCoroutine(TypeWriter(thoughtTmp, thoughts, charDelay));

        IEnumerator TypeWriter(TextMeshProUGUI tmp, string dialogue, float delay)
        {
            var wait = new WaitForSeconds(delay);

            tmp.text = dialogue;
            tmp.maxVisibleCharacters = 0;

            for (int i = 0; i <= dialogue.Length; i++)
            {
                if (_InputManager.skipWriterEffect)
                {
                    tmp.maxVisibleCharacters = dialogue.Length;
                    yield break;
                }

                tmp.maxVisibleCharacters = i;
                yield return wait;
            }
        }
        #endregion
    }
}
