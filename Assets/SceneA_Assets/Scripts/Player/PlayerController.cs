using UnityEngine;

/// <summary>
/// PlayerController - Handles player movement, physics, and animations.
/// Uses CharacterController for collision-based movement (not Rigidbody).
/// Implements camera-relative movement (moves relative to camera direction, not world axes).
/// 
/// Components Required:
/// - CharacterController (on same GameObject)
/// - Animator (on child GameObject - typically the mesh/armature)
/// - Main Camera (tagged as "MainCamera" in scene)
/// 
/// Dependencies:
/// - InputsManager (singleton) for reading movement input
/// </summary>
public class PlayerController : MonoBehaviour
{
    // ==================== COMPONENT REFERENCES ====================

    /// <summary>
    /// Unity's built-in character controller component
    /// Handles collision detection, ground checking, and slope handling
    /// Unlike Rigidbody, this is kinematic (no physics forces, just direct movement)
    /// </summary>
    private CharacterController characterController;

    /// <summary>
    /// Reference to the main camera (used for camera-relative movement)
    /// Camera direction determines which way "forward" is for the player
    /// </summary>
    private Camera mainCam;

    // ==================== INSPECTOR SETTINGS ====================

    [Header("Player Settings")]
    [Tooltip("Base movement speed in units per second")]
    public float speed = 2f;

    [Header("Gravity Settings")]
    [Tooltip("Gravity strength. Can enter as positive value (9.81) and it will be inverted automatically")]

    /// <summary>
    /// [Range Attribute] - Creates a slider in the Unity Inspector
    /// Restricts gravity value between 5f (minimum) and 20f (maximum)
    /// Prevents unrealistic values like 0.1f (moon walking) or 100f (instant drop)
    /// 
    /// Visual: Shows as a draggable slider instead of text field
    /// Usage: Drag slider or click value to type manually within range
    /// 
    /// Recommended values:
    /// - 9.81f: Realistic Earth gravity (default)
    /// - 5f-8f: Low gravity (sci-fi/moon-like feel)
    /// - 15f-20f: Heavy gravity (ground pound feel)
    /// </summary>
    [Range(5f, 20f)] public float gravity = 9.81f;

    [Header("Sprint Settings")]

    /// <summary>
    /// Speed multiplier applied when sprinting is active
    /// Final sprint speed = base speed × sprintMultiplier
    /// Example: speed=2f, sprintMultiplier=2f → Sprint speed = 4f (2× faster)
    /// 
    /// Common values:
    /// - 1.5f: Slight speed boost (realistic jog)
    /// - 2.0f: Standard sprint (typical FPS games)
    /// - 2.5f-3.0f: Fast sprint (arcade/hero games)
    /// 
    /// Controlled by InputsManager.isSprinting toggle state
    /// </summary>
    [Tooltip("Multiplier to increase speed when player is sprinting")]
    public float sprintMultiplier = 2f;

    // ==================== MOVEMENT VECTORS ====================

    /// <summary>
    /// Current velocity vector used for movement calculation
    /// x = horizontal input (left/right)
    /// y = vertical velocity (gravity/falling)
    /// z = vertical input (forward/backward)
    /// </summary>
    private Vector3 velocity;

    /// <summary>
    /// Final movement direction after converting to camera-relative space
    /// This is the actual direction the character will move in world space
    /// </summary>
    private Vector3 moveDirection;

    /// <summary>
    /// Combined final movement vector sent to CharacterController.Move()
    /// Contains: camera-relative X/Z movement + gravity Y velocity
    /// </summary>
    private Vector3 finalMovement;

    /// <summary>
    /// Camera's forward direction (flattened to XZ plane, normalized)
    /// Used to determine where "forward" is relative to camera
    /// </summary>
    private Vector3 mainCamForward;

    /// <summary>
    /// Camera's right direction (flattened to XZ plane, normalized)
    /// Used to determine where "right" is relative to camera
    /// </summary>
    private Vector3 mainCamRight;

    // ==================== CONFIGURATION VALUES ====================

    /// <summary>
    /// Fallback default speed if inspector value is invalid (<= 0)
    /// </summary>
    private float defaultSpeed = 2f;

    /// <summary>
    /// Fallback default sprint multiplier if inspector value is invalid (<= 0)
    /// Used in Awake() to validate sprintMultiplier from Inspector
    /// Ensures sprint always works even if user enters bad value
    /// </summary>
    private float defaultSprintMultiplier = 2f;

    /// <summary>
    /// Small downward force applied when grounded
    /// Keeps character snapped to ground (prevents floating/bouncing)
    /// Negative value applies constant slight downward pressure
    /// </summary>
    private float defaultGravity = -2f;

    /// <summary>
    /// Magnitude (length) of the horizontal movement direction
    /// Used to determine if player is moving for animation purposes
    /// Range: 0 (idle) to ~1.414 (moving diagonally at full input)
    /// </summary>
    private float moveDirMagnitude;

    /// <summary>
    /// Dynamic speed value that changes based on sprint state
    /// Updated every frame in HandleMovement()
    /// 
    /// Values:
    /// - When NOT sprinting: currentSpeed = speed (base walk speed)
    /// - When sprinting: currentSpeed = speed × sprintMultiplier (boosted speed)
    /// 
    /// Used in final calculation: characterController.Move(finalMovement * currentSpeed * Time.deltaTime)
    /// Separating this allows easy expansion (crouch speed, slow-walk, etc.)
    /// </summary>
    private float currentSpeed;

    //Stores the default position of the player 
    private Vector3 defaultPlayerPosition;

    // ==================================================================
    //                          INITIALIZATION
    // ==================================================================

    /// <summary>
    /// Awake is called when the script instance is first loaded
    /// Used to get references to required components before any Start() methods run
    /// This ensures components are available even if other scripts need them in Start()
    /// </summary>
    private void Awake()
    {
        // Get CharacterController from this GameObject
        // Required for collision-based movement
        characterController = GetComponent<CharacterController>();
        if (!characterController) Debug.LogError("No CharacterController found on Player!");

        // Get main camera using Unity's tag system
        // Camera.main finds the camera tagged "MainCamera" in the scene
        mainCam = Camera.main;
        if (!mainCam) Debug.LogError("No Main Camera found! Tag a camera as 'MainCamera'");

        // Validate sprint multiplier - ensure it's positive and functional
        // If user enters 0 or negative in inspector, use default (2f)
        // This prevents sprint from being disabled or causing reverse-speed bugs
        if (sprintMultiplier <= 0) sprintMultiplier = defaultSprintMultiplier;
    }

    /// <summary>
    /// Start is called on the frame when the script is enabled (after Awake)
    /// Used for initial setup and configuration validation
    /// </summary>
    private void Start()
    {

        // Validate speed - ensure it's positive
        // If user enters 0 or negative in inspector, use default
        if (speed <= 0)
        {
            Debug.LogWarning($"Invalid speed ({speed}), using default: {defaultSpeed}");
            speed = defaultSpeed;
        }

        // Ensure gravity is negative (downward force)
        // User can enter 9.81 (positive) and we convert to -9.81
        // If already negative, keep it as-is
        gravity = (gravity > 0) ? -gravity : gravity;

        // Sets the vector 3 data without creating a new vector 3
        defaultPlayerPosition.Set(0f, 1.1f, 0f);
    }

    // ==================================================================
    //                         MAIN GAME LOOP
    // ==================================================================

    /// <summary>
    /// Called every frame (typically 60 times per second)
    /// Main game loop - handles all per-frame logic
    /// Order: Movement first, then animations based on movement
    /// </summary>
    private void Update()
    {
        // Process movement input, camera-relative conversion, and physics
        HandleMovement();

        // Resets the position of the player 
        PositionReset(InputsManager.Instance.resetPosition);
    }

    // ==================================================================
    //                         MOVEMENT SYSTEM
    // ==================================================================

    /// <summary>
    /// Core movement handler - processes input and moves the character
    /// 
    /// FLOW:
    /// 1. Apply gravity to velocity.y
    /// 2. Read raw input from InputsManager into velocity.xz
    /// 3. Get camera forward/right vectors (flattened to ground plane)
    /// 4. Transform input from local/camera space to world space
    /// 5. Determine current speed (walk vs sprint)
    /// 6. Combine horizontal movement + vertical gravity
    /// 7. Move character using CharacterController
    /// </summary>
    void HandleMovement()
    {
        // Step 1: Apply gravity
        // Modifies velocity.y based on grounded state or falling
        HandleGravity();

        // Step 2: Read movement input from centralized InputsManager
        // InputsManager reads WASD/Arrow keys or Left Analog Stick
        // x = -1 (left/A) to +1 (right/D)
        // z = -1 (down/S) to +1 (up/W)  [Note: using Y component of Vector2]
        velocity.x = InputsManager.Instance.movementInput.x;
        velocity.z = InputsManager.Instance.movementInput.y;

        // Step 3: Extract camera direction vectors
        // These represent where the camera is looking (forward) and its right side
        mainCamForward = mainCam.transform.forward;  // Camera's forward vector
        mainCamRight = mainCam.transform.right;      // Camera's right vector

        // Step 4: Flatten vectors to XZ plane (remove Y component)
        // This ensures movement stays horizontal - no flying up/down hills via input!
        // Without this, looking up would make "forward" point upward
        mainCamForward.y = 0f;
        mainCamRight.y = 0f;

        // Normalize vectors to ensure consistent speed in all directions
        // Prevents moving faster when looking at angles (diagonal speed boost)
        // Normalized vector has length of exactly 1.0
        mainCamForward.Normalize();
        mainCamRight.Normalize();

        // Step 5: Calculate camera-relative movement direction
        // FORMULA: moveDirection = (cameraRight * inputX) + (cameraForward * inputZ)
        //
        // EXAMPLE: Player presses W (forward, inputZ = 1) while camera faces North
        //   moveDirection = (cameraRight * 0) + (cameraNorth * 1)
        //   moveDirection = cameraNorth → Character moves North ✓
        //
        // EXAMPLE: Player presses W while camera faces East  
        //   moveDirection = (cameraEastRight * 0) + (cameraEastForward * 1)
        //   moveDirection = East → Character moves East (relative to camera) ✓
        //
        // This allows intuitive controls where "W" always moves "forward" on screen
        moveDirection = mainCamRight * velocity.x + mainCamForward * velocity.z;

        // Step 6: Determine current movement speed based on sprint state
        // Checks InputsManager.isSprinting boolean (toggled by sprint key binding)
        //
        // WHEN SPRINTING (isSprinting = true):
        //   currentSpeed = speed × sprintMultiplier
        //   Example: 2f × 2f = 4f units/sec (fast!)
        //
        // WHEN WALKING (isSprinting = false):
        //   currentSpeed = speed (normal walking pace)
        //   Example: 2f units/sec (normal speed)
        //
        // This approach allows easy extension:
        //   - Add crouch: currentSpeed = speed × crouchMultiplier
        //   - Add slow-walk: currentSpeed = speed × 0.5f
        //   - Add stamina system: modify multiplier based on stamina remaining
        currentSpeed = InputsManager.Instance.isSprinting ? speed * sprintMultiplier : speed;

        // Step 7: Construct final movement vector
        // X and Z come from camera-relative movement direction
        // Y comes from gravity calculation (falling/jumping)
        finalMovement.Set(moveDirection.x, velocity.y, moveDirection.z);

        // Step 8: Apply movement to CharacterController
        // Move() takes absolute movement (not force like Rigidbody.AddForce)
        // Multiply by currentSpeed (walk or sprint) and Time.deltaTime (frame time normalization)
        // This ensures consistent movement regardless of framerate
        characterController.Move(finalMovement * currentSpeed * Time.deltaTime);
    }

    // ==================================================================
    //                           GRAVITY SYSTEM
    // ==================================================================

    /// <summary>
    /// Handles gravity and ground-checking for the character
    /// CharacterController doesn't have built-in gravity like Rigidbody
    /// We must manually apply downward force every frame
    /// 
    /// GRAVITY STATES:
    /// 1. GROUNDED: Apply small negative force (-2f) to keep character stuck to ground
    ///    - Prevents floating when walking over small seams/edges
    ///    - Ensures isGrounded stays true (CharacterController needs this)
    ///    
    /// 2. AIRBORNE: Accumulate gravity force (velocity increases downward)
    ///    - Creates accelerating fall (realistic physics)
    ///    - velocity.y gets more negative each frame until landing
    /// </summary>
    void HandleGravity()
    {
        // Check if character is currently on the ground
        // CharacterController.isGrounded uses a small raycast beneath the character
        // Returns true when standing on any collider (ground, platforms, etc.)
        if (characterController.isGrounded)
        {
            // === GROUNDED STATE ===
            // Reset/set Y velocity to small negative value
            // This keeps the character "snapped" to the ground
            // Without this, the character might:
            //   - Float slightly above ground (isGrounded becomes false)
            //   - Trigger falling animation briefly when walking down slopes
            //   - Bounce on uneven terrain
            //
            // Why -2f and not 0?
            // CharacterController ground detection needs slight downward pressure
            // to maintain contact with the ground collider
            velocity.y = defaultGravity; // -2f by default
        }
        else
        {
            // === AIRBORNE STATE ===
            // Apply accumulating gravity force
            // Each frame, velocity.y decreases (becomes more negative)
            // This creates acceleration: falls faster over time
            //
            // Formula: v = v + (g × Δt)
            // Example with gravity = -9.81, deltaTime = 0.016 (60fps):
            //   Frame 1: velocity.y = 0 + (-9.81 × 0.016) = -0.157
            //   Frame 2: velocity.y = -0.157 + (-0.157) = -0.314
            //   Frame 3: velocity.y = -0.314 + (-0.157) = -0.471
            //   ...continues accelerating until grounded...
            //
            // This creates realistic parabolic fall trajectory!
            velocity.y += gravity * Time.deltaTime;
        }
    }

    /// <summary>
    /// Resets the postion of the player game object 
    /// </summary>
    /// <param name="input"></param>
    void PositionReset(bool input)
    {
        //Perform action when the value is true
        if (input)
        {
            //Turns off the CHARACTER CONTROLLER to prevent any weird behaviour
            characterController.enabled = false;

            //Sets the current postion to the default position
            transform.position = defaultPlayerPosition;

            //After change the position re-enable the CHARACTER CONTROLLER at new position
            characterController.enabled = true;
        }
    }
}