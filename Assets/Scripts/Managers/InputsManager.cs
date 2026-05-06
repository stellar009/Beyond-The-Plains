using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// InputsManager - Centralized input handling system using Unity's new Input System.
/// Implements Singleton pattern to ensure only one instance exists across scenes.
/// Manages all player inputs: movement, camera look, attacks, and pause functionality.
/// 
/// Usage: Access via InputsManager.Instance from any script
/// Example: float moveX = InputsManager.Instance.movementInput.x;
/// </summary>
public class InputsManager : MonoBehaviour
{
    // ==================== SINGLETON INSTANCE ====================
    /// <summary>
    /// Static reference to the single instance of InputsManager
    /// Allows global access from any script without GetComponent calls
    /// </summary>
    public static InputsManager Instance;

    // ==================== INPUT SYSTEM REFERENCE ====================
    /// <summary>
    /// Reference to the generated Input Action Asset (GameInteractions)
    /// This contains all defined action maps and bindings from the Input System window
    /// </summary>
    private GameInteractions gameInteractions;

    // ==================== PUBLIC INPUT PROPERTIES ====================
    /// <summary>
    /// Current movement input vector (WASD/Left Stick)
    /// X = horizontal (-1 left, +1 right)
    /// Y = vertical (-1 down/backward, +1 up/forward)
    /// </summary>
    public Vector2 movementInput { get; private set; }

    /// <summary>
    /// Current camera look input vector (Mouse Delta/Right Stick)
    /// X = horizontal mouse movement (yaw/looking left-right)
    /// Y = vertical mouse movement (pitch/looking up-down)
    /// </summary>
    public Vector2 cameraInputs { get; private set; }

    /// <summary>
    /// Current attack button state (toggle-based)
    /// true = attack active, false = not attacking
    /// Toggles on each button press (not hold-based)
    /// </summary>
    public bool attackState { get; private set; }

    /// <summary>
    /// Current Sprint button state (toggle-based)
    /// true = sprint active, false = not sprinting
    /// Toggles on each button press (not hold-based)
    /// </summary>
    public bool isSprinting { get; private set; }

    public bool changePerspective {  get; private set; }

    // ==================== STATE VARIABLES ====================
    /// <summary>
    /// Tracks whether the game is currently paused
    /// When paused, movement and camera inputs are disabled
    /// </summary>
    private bool isPaused = false;

    // ==================================================================
    //                          INITIALIZATION
    // ==================================================================

    /// <summary>
    /// Awake is called when the script instance is being loaded (before Start)
    /// Used for initialization that must occur before any other scripts run
    /// Sets up the singleton pattern and initial cursor state
    /// </summary>
    private void Awake()
    {
        // Initialize the input action asset class
        // This creates an instance of the auto-generated GameInteractions class
        // which contains all action maps defined in the Input System settings
        gameInteractions = new GameInteractions();

        // Hide cursor at start (typical for FPS/TPS games)
        // Locks cursor to center of screen for unlimited mouse movement
        ShowCursors(false);

        // ========== SINGLETON PATTERN IMPLEMENTATION ==========
        // Check if an instance already exists
        if (Instance == null)
        {
            // No existing instance - this becomes the singleton
            Instance = this;

            // Mark this GameObject as DontDestroyOnLoad
            // Prevents destruction when loading new scenes
            // Ensures input manager persists across entire game
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Instance already exists - destroy this duplicate
            // This prevents multiple input managers causing conflicts
            Destroy(gameObject);
        }
    }

    // ==================================================================
    //                         ENABLE / DISABLE LIFECYCLE
    // ==================================================================

    /// <summary>
    /// Called when the object becomes enabled and active
    /// Subscribes to all input events to begin receiving input data
    /// This is where we "turn on" our input listeners
    /// </summary>
    private void OnEnable()
    {
        // Enable the Player action map
        // This activates all actions within the "Player" action map
        // Actions won't fire events until their map is enabled
        gameInteractions.Player.Enable();
        gameInteractions.Camera.Enable();

        // ========== MOVEMENT INPUT SUBSCRIPTIONS ==========
        // Subscribe to Movement action events
        // "performed" fires when input starts/changes (button press)
        // "canceled" fires when input returns to neutral (button release)
        gameInteractions.Player.Movement.performed += OnMovementPerformend;
        gameInteractions.Player.Movement.canceled += OnMovementCanceled;

        // ========== CAMERA INPUT SUBSCRIPTIONS ==========
        // Subscribe to Camera look action events
        // Typically bound to Mouse Delta or Right Analog Stick
        gameInteractions.Player.Camera.performed += OnCameraInputPerformed;
        gameInteractions.Player.Camera.canceled += OnCameraInputCanceled;

        // ========== PAUSE INPUT SUBSCRIPTION ==========
        // Subscribe to Pause action (Escape key)
        // Uses performed only (no need for cancel - it's a toggle)
        gameInteractions.Player.PauseGame.performed += PauseGame;

        // ========== ATTACK INPUT SUBSCRIPTION ==========
        // Subscribe to Attack action (Left Mouse Click)
        // Toggle-based: each press flips the state
        gameInteractions.Player.Attacks.performed += InAttackState;

        // ========== SPRINT INPUT SUBSCRIPTION ==========
        // Subscribe to Sprint action (Left Shift Key)
        // Toggle-based: each press flips the state
        gameInteractions.Player.Sprint.performed += Sprint;

        gameInteractions.Camera.Perspective.performed += SwapPerspective;
    }

    /// <summary>
    /// Called when the object becomes disabled or destroyed
    /// Unsubscribes from all input events to prevent memory leaks
    /// Always unsubscribe in OnDisable to match subscriptions in OnEnable!
    /// </summary>
    private void OnDisable()
    {
        // Disable the Player action map
        // Stops all input processing for this action map
        gameInteractions.Player.Disable();
        gameInteractions.Camera.Disable();

        // ========== UNSUBSCRIBE FROM ALL EVENTS ==========
        // Must remove all event handlers to prevent:
        // 1. Memory leaks (garbage collector can't clean up orphaned delegates)
        // 2. Null reference exceptions if this object is destroyed
        // 3. Multiple firings if object is re-enabled without proper cleanup

        gameInteractions.Player.Movement.performed -= OnMovementPerformend;
        gameInteractions.Player.Movement.canceled -= OnMovementCanceled;

        gameInteractions.Player.Camera.performed -= OnCameraInputPerformed;
        gameInteractions.Player.Camera.canceled -= OnCameraInputCanceled;

        gameInteractions.Player.PauseGame.performed -= PauseGame;

        gameInteractions.Player.Attacks.performed -= InAttackState;

        gameInteractions.Player.Sprint.performed -= Sprint;

        gameInteractions.Camera.Perspective.performed -= SwapPerspective;
    }

    // ==================================================================
    //                      INPUT CALLBACK HANDLERS
    // ==================================================================

    /// <summary>
    /// Handles Movement input when action is performed (key held/stick moved)
    /// Reads the Vector2 value from the input context and stores it
    /// Called continuously while movement keys are pressed or stick is moved
    /// </summary>
    /// <param name="ctx">Callback context containing input data (value, phase, action, etc.)</param>
    void OnMovementPerformend(InputAction.CallbackContext ctx)
    {
        // Read the Vector2 value from the context
        // For keyboard: returns (-1,0) for A, (1,0) for D, (0,1) for W, etc.
        // For gamepad: returns normalized stick position (-1 to 1 on both axes)
        movementInput = ctx.ReadValue<Vector2>();
    }

    /// <summary>
    /// Handles Movement input when action is canceled (keys released/stick centered)
    /// Resets movement input to zero to stop character movement
    /// Important: Without this, character would continue moving after releasing keys!
    /// </summary>
    /// <param name="ctx">Callback context (value is zero/neutral when canceled)</param>
    void OnMovementCanceled(InputAction.CallbackContext ctx)
    {
        // Reset to zero vector - no movement input
        movementInput = Vector2.zero;
    }

    /// <summary>
    /// Handles Camera look input when action is performed (mouse moving/stick moved)
    /// Reads the Vector2 delta value for camera rotation
    /// For mouse: returns pixel movement since last frame (delta)
    /// For gamepad: returns stick position (not delta - handled differently usually)
    /// </summary>
    /// <param name="ctx">Callback context containing mouse delta or stick position</param>
    void OnCameraInputPerformed(InputAction.CallbackContext ctx)
    {
        // Store camera input for use by PlayerCamera or other systems
        cameraInputs = ctx.ReadValue<Vector2>();
    }

    /// <summary>
    /// Handles Camera look input when action is canceled (mouse stops moving/stick centers)
    /// Resets camera input to zero
    /// Prevents camera drift when user stops moving mouse
    /// </summary>
    /// <param name="ctx">Callback context</param>
    void OnCameraInputCanceled(InputAction.CallbackContext ctx)
    {
        // Clear camera input - no rotation this frame
        cameraInputs = Vector2.zero;
    }

    /// <summary>
    /// Handles Attack button press - TOGGLE implementation
    /// Flips the attack state between true and false on each button press
    /// Note: This is NOT hold-based. Each click toggles the state.
    /// Use this for toggle-able actions like auto-fire or mode switching
    /// </summary>
    /// <param name="ctx">Callback context</param>
    void InAttackState(InputAction.CallbackContext ctx)
    {
        // Toggle boolean: if true becomes false, if false becomes true
        attackState = !attackState;

        // Debug example:
        // Debug.Log($"Attack toggled: {attackState}");
    }

    /// <summary>
    /// Handles Pause button press - TOGGLE implementation
    /// Flips the pause state and calls the PauseGame method to apply changes
    /// Typically bound to Escape key (keyboard)
    /// </summary>
    /// <param name="ctx">Callback context</param>
    void PauseGame(InputAction.CallbackContext ctx)
    {
        // Toggle pause state
        isPaused = !isPaused;

        // Apply pause state changes (disable inputs, show cursor, etc.)
        PauseGame(isPaused);
    }

    /// <summary>
    /// Handles Sprint button press - TOGGLE implementation
    /// Flips the sprint state between true and false on each button press
    /// Note: This is NOT hold-based. Each click toggles the state.
    /// Use this for toggle-able actions like auto-fire or mode switching
    /// </summary>
    /// <param name="ctx">Callback context</param>
    void Sprint(InputAction.CallbackContext ctx)
    {
        // Toggle boolean: if true becomes false, if false becomes true
        isSprinting = !isSprinting;
    }

    void SwapPerspective(InputAction.CallbackContext ctx)
    {
        changePerspective = !changePerspective;
    }

    // ==================================================================
    //                           UTILITY METHODS
    // ==================================================================

    /// <summary>
    /// Pauses or resumes the game by enabling/disabling specific input actions
    /// Can be called externally (from UI buttons, game events, etc.)
    /// 
    /// Logic:
    /// - When PAUSED (true): Keep movement/camera enabled (for menu navigation?)
    ///   NOTE: You might want to disable these during pause - see below
    ///   
    /// - When RESUMED (false): Disable movement and camera (prevent input during pause menu)
    ///   NOTE: The logic here seems inverted - see comments below
    /// </summary>
    /// <param name="state">True = game paused, False = game active</param>
    public void PauseGame(bool state)
    {
        if (state)
        {
            // Currently enables movement/camera when paused (allows menu navigation?)
            // If you want to freeze the player during pause, change these to Disable()
            gameInteractions.Player.Movement.Enable();
            gameInteractions.Player.Camera.Enable();
        }
        else
        {
            // Currently disables movement/camera when unpaused (opposite of expected)
            // If fixing above, change these to Enable()
            gameInteractions.Player.Movement.Disable();
            gameInteractions.Player.Camera.Disable();
        }

        // Show cursor when paused, hide when active
        // Using !state because we want cursor VISIBLE during pause (state=true)
        // and HIDDEN during gameplay (state=false)
        ShowCursors(!state);
    }

    /// <summary>
    /// Controls cursor visibility and lock state
    /// 
    /// When status is TRUE (visible):
    /// - Cursor is shown on screen
    /// - Cursor is unlocked (can move freely, can click UI)
    /// - Use this for menus, pause screens, inventory, etc.
    /// 
    /// When status is FALSE (hidden):
    /// - Cursor is invisible
    /// - Cursor is locked to screen center (confined to game window)
    /// - Use this during gameplay for FPS/TPS camera control
    /// </summary>
    /// <param name="status">True = show and unlock cursor, False = hide and lock cursor</param>
    void ShowCursors(bool status)
    {
        // Set cursor visibility
        Cursor.visible = status;

        // Set cursor lock mode based on status
        // CursorLockMode.None: Free movement, visible (for UI/menus)
        // CursorLockMode.Locked: Locked to center, hidden (for gameplay)
        Cursor.lockState = status ? CursorLockMode.None : CursorLockMode.Locked;
    }
}