using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// InputsManager - Handles all player inputs using Unity's new Input System.
/// Uses Singleton pattern so only one instance exists.
/// 
/// How to use: InputsManager.Instance.movementInput (gets WASD/joystick)
/// </summary>
public class InputsManager : MonoBehaviour
{
    // ==================== SINGLETON INSTANCE ====================

    /// <summary>
    /// The one and only instance of this script.
    /// Access from anywhere: InputsManager.Instance
    /// </summary>
    public static InputsManager Instance;

    // ==================== INPUT SYSTEM REFERENCE ====================

    /// <summary>
    /// The auto-generated Input System class that contains all our actions.
    /// Created from the .inputactions asset.
    /// </summary>
    private GameInteractions m_GameInteractions;

    // ==================== PUBLIC INPUT PROPERTIES ====================

    /// <summary>
    /// Movement input (WASD or left joystick).
    /// X = horizontal (-1 left, +1 right)
    /// Y = vertical (-1 backward, +1 forward)
    /// </summary>
    public Vector2 movementInput { get; private set; }

    /// <summary>
    /// Camera look input (mouse movement or right joystick).
    /// X = look left/right
    /// Y = look up/down
    /// </summary>
    public Vector2 cameraInputs { get; private set; }

    /// <summary>
    /// Attack state - toggles on/off each time you press the attack button.
    /// true = attacking, false = not attacking
    /// </summary>
    public bool attackState { get; private set; }

    /// <summary>
    /// Sprint state - toggles on/off each time you press sprint.
    /// true = sprinting, false = walking
    /// </summary>
    public bool isSprinting { get; private set; }

    /// <summary>
    /// Resets the player position when pressed.
    /// True for a moment, then auto-resets to false.
    /// </summary>
    public bool resetPosition { get; private set; }

    // ==================== STATE VARIABLES ====================

    /// <summary>
    /// Is the game paused? When true, most inputs stop working.
    /// </summary>
    private bool m_IsPaused = false;

    // ==================================================================
    //                          INITIALIZATION
    // ==================================================================

    /// <summary>
    /// Called when the script loads - sets up the singleton and input system.
    /// </summary>
    private void Awake()
    {
        // Create the input system instance
        m_GameInteractions = new GameInteractions();

        // Hide the mouse cursor (standard for FPS games)
        ShowCursor(false);

        // ========== SINGLETON SETUP ==========
        // If no instance exists, this becomes the one and only
        if (Instance == null)
        {
            Instance = this;
            // Keep this object alive when switching scenes
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // There's already an instance - delete this duplicate
            Destroy(gameObject);
        }
    }

    // ==================================================================
    //                         ENABLE / DISABLE LIFECYCLE
    // ==================================================================

    /// <summary>
    /// Called when this object is enabled - starts listening for inputs.
    /// </summary>
    private void OnEnable()
    {
        // Turn on the Player action map (activates all player inputs)
        m_GameInteractions.Player.Enable();

        // ========== CONNECT INPUTS TO THEIR FUNCTIONS ==========
        // Movement (WASD)
        m_GameInteractions.Player.Movement.performed += OnMovementPerformend;
        m_GameInteractions.Player.Movement.canceled += OnMovementCanceled;

        // Camera Look (mouse)
        m_GameInteractions.Player.Camera.performed += OnCameraInputPerformed;
        m_GameInteractions.Player.Camera.canceled += OnCameraInputCanceled;

        // Pause (Escape key)
        m_GameInteractions.Player.PauseGame.performed += PauseGame;

        // Attack (Left Mouse Click)
        m_GameInteractions.Player.Attacks.performed += InAttackState;

        // Sprint (Left Shift)
        m_GameInteractions.Player.Sprint.performed += Sprint;

        // Reset Position (P key)
        m_GameInteractions.Player.PositionReset.performed += ResetPosition;
    }

    /// <summary>
    /// Called when this object is disabled - stops listening for inputs.
    /// Important: Always disconnect events to avoid memory leaks!
    /// </summary>
    private void OnDisable()
    {
        // Turn off the Player action map
        m_GameInteractions.Player.Disable();

        // ========== DISCONNECT ALL INPUTS ==========
        // Remove all event connections
        m_GameInteractions.Player.Movement.performed -= OnMovementPerformend;
        m_GameInteractions.Player.Movement.canceled -= OnMovementCanceled;

        m_GameInteractions.Player.Camera.performed -= OnCameraInputPerformed;
        m_GameInteractions.Player.Camera.canceled -= OnCameraInputCanceled;

        m_GameInteractions.Player.PauseGame.performed -= PauseGame;
        m_GameInteractions.Player.Attacks.performed -= InAttackState;
        m_GameInteractions.Player.Sprint.performed -= Sprint;
        m_GameInteractions.Player.PositionReset.performed -= ResetPosition;
    }

    // ==================================================================
    //                      INPUT CALLBACK HANDLERS
    // ==================================================================

    /// <summary>
    /// Called when movement keys are pressed or joystick is moved.
    /// Stores the input value (direction and strength).
    /// </summary>
    void OnMovementPerformend(InputAction.CallbackContext ctx)
    {
        // Read the input value (Vector2: x=horizontal, y=vertical)
        movementInput = ctx.ReadValue<Vector2>();
    }

    /// <summary>
    /// Called when movement keys are released or joystick centers.
    /// Stops movement by setting input to zero.
    /// </summary>
    void OnMovementCanceled(InputAction.CallbackContext ctx)
    {
        movementInput = Vector2.zero; // Stop moving
    }

    /// <summary>
    /// Called when the mouse moves or right joystick is used.
    /// Stores the look input for camera rotation.
    /// </summary>
    void OnCameraInputPerformed(InputAction.CallbackContext ctx)
    {
        cameraInputs = ctx.ReadValue<Vector2>();
    }

    /// <summary>
    /// Called when mouse stops moving or joystick centers.
    /// Stops camera rotation.
    /// </summary>
    void OnCameraInputCanceled(InputAction.CallbackContext ctx)
    {
        cameraInputs = Vector2.zero; // Stop looking around
    }

    /// <summary>
    /// Called when Attack button is pressed.
    /// Toggles attack on/off (each press flips the state).
    /// </summary>
    void InAttackState(InputAction.CallbackContext ctx)
    {
        attackState = !attackState; // Flip: true→false, false→true
    }

    /// <summary>
    /// Called when Pause button (Escape) is pressed.
    /// Toggles pause on/off.
    /// </summary>
    void PauseGame(InputAction.CallbackContext ctx)
    {
        m_IsPaused = !m_IsPaused; // Toggle pause state
        PauseGame(m_IsPaused);     // Apply pause changes
    }

    /// <summary>
    /// Called when Sprint button (Left Shift) is pressed.
    /// Toggles sprint on/off.
    /// </summary>
    void Sprint(InputAction.CallbackContext ctx)
    {
        isSprinting = !isSprinting; // Flip sprint state
    }

    /// <summary>
    /// Called when Reset Position button (P) is pressed.
    /// Sets resetPosition to true for 1 second, then auto-resets to false.
    /// </summary>
    void ResetPosition(InputAction.CallbackContext ctx)
    {
        resetPosition = !resetPosition; // Set to true
        Invoke(nameof(ResetPlayerPositionButtonState), 1f); // Auto-reset after 1 second
    }

    // ==================================================================
    //                           UTILITY METHODS
    // ==================================================================

    /// <summary>
    /// Pauses or resumes the game.
    /// Controls which inputs are active and shows/hides the cursor.
    /// </summary>
    /// <param name="state">true = paused, false = playing</param>
    public void PauseGame(bool state)
    {
        if (!state)
        {
            // When paused: movement and camera are paused
            m_GameInteractions.Player.Movement.Enable();
            m_GameInteractions.Player.Camera.Enable();
        }
        else
        {
            // When unpaused: movement and camera are Enabled
            m_GameInteractions.Player.Movement.Disable();
            m_GameInteractions.Player.Camera.Disable();
        }

        // Show cursor during pause, hide during gameplay
        ShowCursor(state); // state=true → show cursor, state=false → hide cursor
    }

    /// <summary>
    /// Shows or hides the mouse cursor.
    /// </summary>
    /// <param name="canShow">true = show cursor, false = hide cursor</param>
    void ShowCursor(bool canShow)
    {
        Cursor.visible = canShow; // Show/hide cursor

        // Lock cursor to center when hidden (for gameplay)
        // Allow free movement when visible (for menus)
        Cursor.lockState = canShow ? CursorLockMode.None : CursorLockMode.Locked;
    }

    /// <summary>
    /// Resets the resetPosition button state back to false.
    /// Called automatically 1 second after ResetPosition is pressed.
    /// </summary>
    void ResetPlayerPositionButtonState()
    {
        resetPosition = !resetPosition; // Flip back to false
    }
}