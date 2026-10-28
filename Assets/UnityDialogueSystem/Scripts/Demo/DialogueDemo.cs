using UnityEngine;
using UnityEngine.InputSystem;
using ZaiGames.DialogueSystem.Core;
using ZaiGames.DialogueSystem.Save;

namespace ZaiGames.DialogueSystem.Demo
{
    /// <summary>
    /// Demo bootstrap. On Start(), begins the dialogue via the bound
    /// <see cref="DialogueRunner"/>. During play, exposes quick save /
    /// load / restart keys for testing:
    ///
    /// <list type="bullet">
    ///   <item><b>F5</b>  — save current story state to persistentDataPath</item>
    ///   <item><b>F9</b>  — load saved state and resume the pump</item>
    ///   <item><b>F12</b> — delete save and restart from the beginning</item>
    /// </list>
    ///
    /// In a real game these would be driven by your save-game UI or
    /// checkpoint trigger volumes; this script exists purely to
    /// demonstrate the round-trip.
    /// </summary>
    public class DialogueDemo : MonoBehaviour
    {
        [SerializeField] private DialogueRunner runner;

        [Header("Optional — auto-start dialogue on Awake")]
        [SerializeField] private bool autoStartOnStart = true;

        private async void Start()
        {
            // Wait one frame so the input poller + view have a chance to subscribe
            // to runner events in their OnEnable before we begin.
            await Awaitable.EndOfFrameAsync();

            if (autoStartOnStart)
                await runner.Begin();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || runner == null) return;

            if (kb[Key.F5].wasPressedThisFrame)
            {
                bool ok = DialogueSaveLoad.Save(runner);
                Debug.Log(ok ? "[Demo] F5 — saved." : "[Demo] F5 — save failed.");
            }

            if (kb[Key.F9].wasPressedThisFrame && DialogueSaveLoad.SaveExists())
            {
                bool ok = DialogueSaveLoad.LoadInto(runner);
                if (ok)
                {
                    _ = runner.RestartFromLoadedState();
                    Debug.Log("[Demo] F9 — loaded and resumed.");
                }
            }

            if (kb[Key.F12].wasPressedThisFrame)
            {
                DialogueSaveLoad.DeleteSave();
                _ = runner.Begin();
                Debug.Log("[Demo] F12 — restart from beginning.");
            }
        }
    }
}
