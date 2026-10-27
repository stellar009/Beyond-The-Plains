// ---------------------------------------------------------------------------
// NPCInteractable.cs
// ---------------------------------------------------------------------------
// 3D-world bridge: turns a GameObject in your 3D scene (an NPC, a sign, a
// treasure chest, a door) into a thing the player can interact with to
// start an Ink dialogue.
//
// Two trigger modes, both can be active on the same NPC:
//   1. CLICK   - player clicks the NPC with the mouse. Uses a raycast
//                from the camera through the mouse position. Requires a
//                Collider on the NPC.
//   2. PROXIMITY - player walks into a trigger zone. Uses OnTriggerEnter
//                on a separate trigger collider (usually a child object).
//                Can auto-start or wait for an "Interact" key press.
//
// This script is intentionally framework-light: it uses Unity's legacy
// Input system (Input.GetMouseButtonDown, Input.GetKeyDown) and basic
// Collider/trigger callbacks. For the new Input System or for raycast-
// based interaction managers, swap the input reads -- the public API
// (Interact()) stays the same.
//
// Usage:
//   1. Place this on a 3D NPC model in your scene.
//   2. Give the NPC a Collider (BoxCollider, CapsuleCollider, or
//      MeshCollider) so raycasts can hit it.
//   3. (Optional) Add a child GameObject with a trigger Collider for
//      proximity detection. Drag that child into proximityTrigger.
//   4. In the Inspector, set:
//        storyName = the .ink story to launch (e.g. "sample_story")
//        startKnot = optional knot/divert to enter at (e.g. "stranger_intro")
//        interactionMode = Click | Proximity | Both
//        requireKeyPress = true if proximity should wait for a keypress
//        interactKey = E or F (default E)
// ---------------------------------------------------------------------------

using UnityEngine;
using InkDialogueSystem.UI; // optional, used to show/hide dialogue UI

namespace InkDialogueSystem.Core
{
    /// <summary>
    /// Makes a 3D GameObject start an Ink dialogue when the player clicks
    /// it or walks into its trigger zone. Designed for 3D games; works
    /// equally well for top-down, third-person, or first-person.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class NPCInteractable : MonoBehaviour
    {
        public enum Mode
        {
            Click       = 1,  // mouse-click only
            Proximity   = 2,  // trigger zone only (with optional key press)
            Both        = 3,  // either input starts the dialogue
        }

        [Header("Dialogue")]
        [Tooltip("Name of the compiled .ink.json file (no extension) " +
                 "inside Resources/Ink/ or StreamingAssets/Ink/.")]
        [SerializeField] private string storyName = "sample_story";

        [Tooltip("Optional knot/divert name to enter the story at. " +
                 "Leave empty to start from the top. Example: " +
                 "\"stranger_intro\" will jump straight to that knot.")]
        [SerializeField] private string startKnot = "";

        [Tooltip("If true, the dialogue UI GameObject (assigned below) " +
                 "is activated when the dialogue starts and deactivated " +
                 "when it ends. Leave the field empty if you want the " +
                 "UI to be always visible.")]
        [SerializeField] private GameObject dialogueUIRoot;

        [Header("Interaction")]
        [Tooltip("How the player triggers this NPC.")]
        [SerializeField] private Mode interactionMode = Mode.Both;

        [Tooltip("Layer mask for the click raycast. Set to whatever " +
                 "layer your NPCs live on. Default = Everything.")]
        [SerializeField] private LayerMask clickLayerMask = ~0;

        [Tooltip("Max distance the click raycast travels, in world units.")]
        [SerializeField] private float clickMaxDistance = 100f;

        [Header("Proximity")]
        [Tooltip("Trigger collider that defines the interaction zone. " +
                 "If empty, this script looks for a trigger Collider on " +
                 "a child named 'InteractionZone'.")]
        [SerializeField] private Collider proximityTrigger;

        [Tooltip("If true, walking into the trigger is not enough -- the " +
                 "player must also press interactKey. Shows an interaction " +
                 "prompt while inside the zone.")]
        [SerializeField] private bool requireKeyPress = true;

        [Tooltip("Key the player presses to interact when inside the " +
                 "trigger zone (only used if requireKeyPress is true).")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Tooltip("UI element shown when the player is inside the trigger " +
                 "zone and can interact. Optional -- leave empty if you " +
                 "don't want a prompt.")]
        [SerializeField] private GameObject interactionPrompt;

        // -----------------------------------------------------------------
        // State
        // -----------------------------------------------------------------

        private Camera _mainCamera;
        private bool _playerInsideTrigger;
        private bool _hasStartedDialogue;

        // -----------------------------------------------------------------
        // Lifecycle
        // -----------------------------------------------------------------

        private void Awake()
        {
            _mainCamera = Camera.main;

            // If no proximity trigger was assigned, try to find one on a
            // child named "InteractionZone". This convention makes it
            // easy to drop an NPC prefab into a scene without manual setup.
            if (proximityTrigger == null)
            {
                Transform zone = transform.Find("InteractionZone");
                if (zone != null) proximityTrigger = zone.GetComponent<Collider>();
            }

            // Ensure the assigned trigger is actually a trigger.
            if (proximityTrigger != null)
            {
                proximityTrigger.isTrigger = true;
            }

            if (interactionPrompt != null) interactionPrompt.SetActive(false);
            if (dialogueUIRoot != null) dialogueUIRoot.SetActive(false);

            DialogueEvents.OnDialogueEnded += HandleDialogueEnded;
        }

        private void OnDestroy()
        {
            DialogueEvents.OnDialogueEnded -= HandleDialogueEnded;
        }

        private void Update()
        {
            if (_hasStartedDialogue) return;

            // Click mode: raycast from camera through mouse.
            if (interactionMode == Mode.Click || interactionMode == Mode.Both)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    TryHandleClick();
                }
            }

            // Proximity mode with key press required.
            if ((interactionMode == Mode.Proximity || interactionMode == Mode.Both)
                && _playerInsideTrigger && requireKeyPress)
            {
                if (Input.GetKeyDown(interactKey))
                {
                    StartDialogue();
                }
            }
        }

        // -----------------------------------------------------------------
        // Trigger callbacks (proximity mode)
        // -----------------------------------------------------------------

        private void OnTriggerEnter(Collider other)
        {
            if (!IsPlayer(other)) return;
            _playerInsideTrigger = true;
            if (interactionPrompt != null) interactionPrompt.SetActive(true);

            // Auto-start if no key press required.
            if (!requireKeyPress && (interactionMode == Mode.Proximity || interactionMode == Mode.Both))
            {
                StartDialogue();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsPlayer(other)) return;
            _playerInsideTrigger = false;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
        }

        // -----------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------

        /// <summary>Manually start the dialogue (e.g. from a UI button or
        /// a custom interaction system). This is the entry point used by
        /// all internal triggers too.</summary>
        public void StartDialogue()
        {
            if (_hasStartedDialogue) return;
            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[NPCInteractable] No active DialogueManager in scene.");
                return;
            }

            _hasStartedDialogue = true;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);

            // Show the dialogue UI if one was assigned.
            if (dialogueUIRoot != null) dialogueUIRoot.SetActive(true);

            // Load the story (idempotent if the right story is already loaded).
            if (DialogueManager.Instance.CurrentStoryName != storyName)
            {
                if (!DialogueManager.Instance.StartStory(storyName))
                {
                    _hasStartedDialogue = false;
                    if (dialogueUIRoot != null) dialogueUIRoot.SetActive(false);
                    return;
                }
            }

            // Optionally jump to a specific knot/divert.
            if (!string.IsNullOrEmpty(startKnot))
            {
                DialogueManager.Instance.Story?.ChoosePathString(startKnot);
            }

            // Pump the first line.
            DialogueManager.Instance.Continue();
        }

        // -----------------------------------------------------------------
        // Internals
        // -----------------------------------------------------------------

        private void TryHandleClick()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, clickMaxDistance, clickLayerMask))
            {
                if (hit.collider != null && hit.collider.transform.IsChildOf(transform))
                {
                    StartDialogue();
                }
            }
        }

        /// <summary>Check whether the collider that entered/exited the
        /// trigger belongs to the player. Override this for custom player
        /// detection (e.g. tag check, layer check, component check).</summary>
        protected virtual bool IsPlayer(Collider other)
        {
            // Default heuristic: tag "Player". Replace with whatever your
            // player detection strategy is -- e.g.:
            //   return other.GetComponentInParent<PlayerController>() != null;
            return other.CompareTag("Player");
        }

        private void HandleDialogueEnded()
        {
            // The story finished. Reset so this NPC can be interacted
            // with again later (e.g. re-talking to an NPC after the
            // conversation ends).
            _hasStartedDialogue = false;
            if (dialogueUIRoot != null) dialogueUIRoot.SetActive(false);
        }
    }
}
