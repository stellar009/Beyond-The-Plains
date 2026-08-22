using UnityEngine;

/// <summary>
/// PlayerController - Moves the player and handles animations.
/// Uses CharacterController (not Rigidbody) for movement.
/// Moves relative to where the camera is looking.
/// 
/// Needs these on the player:
/// - CharacterController
/// - A camera tagged "MainCamera" in the scene
/// 
/// Uses InputsManager to get keyboard/controller input.
/// </summary>
public class PlayerController : MonoBehaviour
{
    // ==================== COMPONENT REFERENCES ====================

    /// <summary>
    /// The CharacterController handles collision and ground detection.
    /// It moves the player without using physics forces.
    /// </summary>
    private CharacterController m_CharacterController;

    /// <summary>
    /// The main camera - used to figure out which direction is "forward" on screen.
    /// </summary>
    private Camera m_MainCamera;

    // ==================== INSPECTOR SETTINGS ====================

    [Header("Player Settings")]
    [Tooltip("How fast the player walks (units per second)")]
    [SerializeField] private float m_Speed = 2f;

    [Header("Gravity Settings")]
    [Tooltip("How strong gravity is. 9.81 = normal Earth gravity")]

    /// <summary>
    /// [Range] makes a slider in the Inspector - values between 5 and 20.
    /// Lower = floaty, higher = heavy.
    /// </summary>
    [Range(5f, 20f)] [SerializeField] private float m_Gravity = 9.81f;

    [Header("Sprint Settings")]

    /// <summary>
    /// How much faster sprinting is compared to walking.
    /// Example: speed=2, multiplier=2 → sprint speed = 4.
    /// </summary>
    [Tooltip("Multiplier for sprint speed (2 = twice as fast)")]
    [SerializeField] private float m_SprintMultiplier = 2f;

    // ==================== MOVEMENT VARIABLES ====================

    /// <summary>
    /// Current velocity (direction + speed).
    /// X = left/right, Y = up/down (gravity), Z = forward/backward.
    /// </summary>
    private Vector3 m_Velocity;

    /// <summary>
    /// The direction the player will move, relative to the camera.
    /// </summary>
    private Vector3 m_MoveDirection;

    /// <summary>
    /// The final movement vector sent to CharacterController.Move().
    /// Combines horizontal movement + vertical gravity.
    /// </summary>
    private Vector3 m_FinalMovement;

    /// <summary>
    /// Camera's forward direction (flattened so it's horizontal).
    /// </summary>
    private Vector3 m_MainCamForward;

    /// <summary>
    /// Camera's right direction (flattened so it's horizontal).
    /// </summary>
    private Vector3 m_MainCamRight;

    // ==================== DEFAULT VALUES ====================

    /// <summary>
    /// Fallback speed if inspector value is invalid (0 or negative).
    /// </summary>
    private float k_DefaultSpeed = 2f;

    /// <summary>
    /// Fallback sprint multiplier if inspector value is invalid.
    /// </summary>
    private float k_DefaultSprintMultiplier = 2f;

    /// <summary>
    /// Small downward force applied when grounded - keeps the player stuck to the floor.
    /// </summary>
    private float k_DefaultGravity = -2f;

    /// <summary>
    /// Current speed - changes between walk speed and sprint speed.
    /// </summary>
    private float m_CurrentSpeed;

    /// <summary>
    /// Where the player starts - used for resetting position.
    /// </summary>
    private Vector3 k_DefaultPlayerPosition;

    // ==================================================================
    //                          INITIALIZATION
    // ==================================================================

    /// <summary>
    /// Called when the script first loads - gets all required components.
    /// </summary>
    private void Awake()
    {
        // Get the CharacterController component from this GameObject
        m_CharacterController = GetComponent<CharacterController>();
        if (!m_CharacterController) Debug.LogError("No CharacterController found on Player!");

        // Find the main camera by its tag
        m_MainCamera = Camera.main;
        if (!m_MainCamera) Debug.LogError("No Main Camera found! Tag a camera as 'MainCamera'");

        // Make sure sprint multiplier is valid (if not, use default)
        if (m_SprintMultiplier <= 0) m_SprintMultiplier = k_DefaultSprintMultiplier;
    }

    /// <summary>
    /// Called after Awake - sets up initial values.
    /// </summary>
    private void Start()
    {
        // Make sure speed is valid
        if (m_Speed <= 0)
        {
            Debug.LogWarning($"Invalid speed ({m_Speed}), using default: {k_DefaultSpeed}");
            m_Speed = k_DefaultSpeed;
        }

        // Convert gravity to negative (downward) if the user entered it as positive
        m_Gravity = (m_Gravity > 0) ? -m_Gravity : m_Gravity;

        // Set the default position (where the player respawns)
        k_DefaultPlayerPosition.Set(0f, 1.1f, 0f);
    }

    // ==================================================================
    //                         MAIN GAME LOOP
    // ==================================================================

    /// <summary>
    /// Called every frame - handles movement and position reset.
    /// </summary>
    private void Update()
    {
        // Move the player based on input
        HandleMovement();
    }

    // ==================================================================
    //                         MOVEMENT SYSTEM
    // ==================================================================

    /// <summary>
    /// Reads input, calculates movement direction, and moves the player.
    /// 
    /// How it works:
    /// 1. Apply gravity (falling or staying on ground)
    /// 2. Read WASD/joystick input
    /// 3. Figure out which way the camera is looking
    /// 4. Convert input from "relative to camera" to "world space"
    /// 5. Check if sprinting (faster movement)
    /// 6. Move the character
    /// </summary>
    void HandleMovement()
    {
        // Step 1: Apply gravity (fall or stay grounded)
        HandleGravity();

        // Step 2: Get input from InputsManager
        // x = left/right (-1 to 1), y = forward/backward (-1 to 1)
        m_Velocity.x = InputsManager.Instance.movementInput.x;
        m_Velocity.z = InputsManager.Instance.movementInput.y;

        // Step 3: Get camera's forward and right directions
        m_MainCamForward = m_MainCamera.transform.forward;
        m_MainCamRight = m_MainCamera.transform.right;

        // Step 4: Flatten vectors (ignore up/down so we don't fly into the air)
        m_MainCamForward.y = 0f;
        m_MainCamRight.y = 0f;

        // Normalize so diagonal movement isn't faster than straight
        m_MainCamForward.Normalize();
        m_MainCamRight.Normalize();

        // Step 5: Calculate movement direction relative to camera
        // Example: Press W (forward) while camera faces North → move North
        // Press W while camera faces East → move East
        m_MoveDirection = m_MainCamRight * m_Velocity.x + m_MainCamForward * m_Velocity.z;

        // Step 6: Decide if we're walking or sprinting
        m_CurrentSpeed = InputsManager.Instance.isSprinting ? m_Speed * m_SprintMultiplier : m_Speed;

        // Step 7: Combine horizontal movement with vertical gravity
        m_FinalMovement.Set(m_MoveDirection.x, m_Velocity.y, m_MoveDirection.z);

        // Step 8: Actually move the character
        // Move() moves the character by this amount this frame
        m_CharacterController.Move(m_FinalMovement * m_CurrentSpeed * Time.deltaTime);
    }

    // ==================================================================
    //                           GRAVITY SYSTEM
    // ==================================================================

    /// <summary>
    /// Handles gravity - keeps the player on the ground or makes them fall.
    /// 
    /// Grounded: applies a tiny downward force to keep contact with the ground.
    /// Airborne: accumulates gravity (falls faster over time).
    /// </summary>
    void HandleGravity()
    {
        // Check if the player is standing on something
        if (m_CharacterController.isGrounded)
        {
            // On ground: small downward force to stay stuck to floor
            m_Velocity.y = k_DefaultGravity; // -2f
        }
        else
        {
            // In air: gravity pulls you down, getting faster each frame
            // Example: 0 → -0.157 → -0.314 → -0.471 → (accelerating fall)
            m_Velocity.y += m_Gravity * Time.deltaTime;
        }
    }

    /// <summary>
    /// Resets the player to their starting position.
    /// Turns off the CharacterController temporarily to avoid issues.
    /// </summary>
    /// <param name="shouldReset">True when reset button is pressed</param>
    void PositionReset(bool shouldReset)
    {
        if (shouldReset)
        {
            // Turn off controller so it doesn't interfere with position change
            m_CharacterController.enabled = false;

            // Move player back to start
            transform.position = k_DefaultPlayerPosition;

            // Turn controller back on at the new position
            m_CharacterController.enabled = true;
        }
    }
}