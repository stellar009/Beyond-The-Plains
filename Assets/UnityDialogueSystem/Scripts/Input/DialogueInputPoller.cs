using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using ZaiGames.DialogueSystem.Core;

namespace ZaiGames.DialogueSystem.Input
{
    /// <summary>
    /// Polls the New Input System every frame via <c>Keyboard.current</c>
    /// and <c>Mouse.current</c>, dispatching advance / skip / choice
    /// actions to a bound <see cref="DialogueRunner"/>.
    ///
    /// Why polling (not InputAction asset): the user picked the
    /// "Polling only" wiring style. This keeps the system dependency-
    /// free — no InputActions asset to generate, no PlayerInput
    /// component to configure. Keyboard + Mouse only.
    ///
    /// Bindings:
    /// - Advance / Skip: <see cref="advanceKey"/> (default Space),
    ///   <see cref="altAdvanceKey"/> (Enter), or
    ///   <see cref="advanceMouse"/> (Left click).
    /// - Choice 1..N: digits <c>D1</c>..<c>D9</c> on the top row.
    ///
    /// An <see cref="advanceCooldown"/> prevents double-trigger when the
    /// key is held across frames.
    /// </summary>
    public class DialogueInputPoller : MonoBehaviour
    {
        [Header("Bound runner")]
        [SerializeField] private DialogueRunner runner;

        [Header("Advance / Skip bindings")]
        [SerializeField] private Key advanceKey = Key.Space;
        [SerializeField] private Key altAdvanceKey = Key.Enter;
        [SerializeField] private MouseButton advanceMouse = MouseButton.Left;

        [Header("Choice number keys (top-row digits)")]
        [SerializeField] private Key[] choiceKeys =
        {
            Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9
        };

        [Header("Tuning")]
        [Tooltip("Seconds between accepted advance presses; prevents double-trigger on hold.")]
        [SerializeField] private float advanceCooldown = 0.15f;

        [Tooltip("If true, advance during WaitingForChoice does nothing (must pick a choice). If false, advance picks the first choice.")]
        [SerializeField] private bool blockAdvanceDuringChoices = true;

        private float _lastAdvanceTime = -1f;

        private void Update()
        {
            if (runner == null) return;
            var kb = Keyboard.current;
            var mouse = Mouse.current;

            // ----- Choice selection (only when waiting for choice) -----
            if (runner.Phase == DialoguePhase.WaitingForChoice && kb != null)
            {
                for (int i = 0; i < choiceKeys.Length; i++)
                {
                    if (kb[choiceKeys[i]].wasPressedThisFrame)
                    {
                        runner.SelectChoice(i);
                        return;
                    }
                }
            }

            // ----- Advance / Skip -----
            bool advancePressed = false;
            if (kb != null)
            {
                advancePressed |= kb[advanceKey].wasPressedThisFrame;
                advancePressed |= kb[altAdvanceKey].wasPressedThisFrame;
            }
            if (mouse != null)
            {
                advancePressed |= WasPressedThisFrame(mouse, advanceMouse);
            }

            if (!advancePressed) return;
            if (Time.unscaledTime - _lastAdvanceTime < advanceCooldown) return;
            _lastAdvanceTime = Time.unscaledTime;

            switch (runner.Phase)
            {
                case DialoguePhase.Typing:
                    runner.RequestSkipTypewriter();
                    break;

                case DialoguePhase.WaitingForContinue:
                    runner.RequestAdvance();
                    break;

                case DialoguePhase.WaitingForChoice:
                    if (!blockAdvanceDuringChoices && runner.Story != null
                        && runner.Story.currentChoices.Count > 0)
                    {
                        runner.SelectChoice(0);
                    }
                    break;
            }
        }
        private static bool WasPressedThisFrame(Mouse mouse, MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left: return mouse.leftButton.wasPressedThisFrame;
                case MouseButton.Right: return mouse.rightButton.wasPressedThisFrame;
                case MouseButton.Middle: return mouse.middleButton.wasPressedThisFrame;
                case MouseButton.Forward: return mouse.forwardButton.wasPressedThisFrame;
                case MouseButton.Back: return mouse.backButton.wasPressedThisFrame;
                default: return false;
            }
        }
    }

}
