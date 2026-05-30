using UnityEngine;

/// <summary>
/// PlayerCamera - Handles first-person camera rotation and mouse look functionality.
/// Supports seamless switching between First Person (FPP) and Third Person (TPP) perspectives.
/// Attached to the camera object which should be a child of the player body.
/// </summary>
public class PlayerCamera : MonoBehaviour
{
    [Header("Global Perspective Settings")]
    [Tooltip("Time in seconds for camera to smoothly transition between perspectives")]
    [Range(0.01f, 2f)] public float smoothTime = 0.25f;
    // Lower value (0.1f) = Snappy, fast transition
    // Higher value (1.0f) = Slow, cinematic transition
    // 0.25f = Good balance (responsive but smooth)

    [Header("Third Person Perspective Settings")]
    [Tooltip("How far behind the player camera sits in TPP mode")]
    [Range(1f, 5f)] public float distanceTPP = 2f;
    // Camera will be positioned this many units behind the player
    // 2f = Close over-shoulder view (like modern action games)
    // 5f = Far camera (like older RPGs, better situational awareness)

    [Header("Mouse Settings")]
    [Tooltip("Mouse sensitivity multiplier for camera rotation speed")]
    public float sensitivity = 10f;

    [Tooltip("Maximum upward look angle in degrees")]
    public float maxLookAngle = 90f;

    [Tooltip("Minimum downward look angle in degrees")]
    public float minLookAngle = -60f;

    /// <summary>
    /// Tracks which perspective mode is currently active
    /// Set by InputsManager when player toggles perspective key
    /// true = Third Person Perspective (camera behind player)
    /// false = First Person Perspective (camera inside player head)
    /// </summary>
    private bool isTPP;

    // Reference to the player body transform (parent object)
    private Transform playerBody;

    // Current vertical rotation (up/down looking) - applied to camera locally
    private float xRotation = 0f;

    // Current horizontal rotation (left/right looking) - applied to player body
    private float yRotation = 0f;

    // Raw mouse input values from input manager
    private float mouseX, mouseY;

    /// <summary>
    /// Camera distance for First Person mode
    /// Always 0 because FPP camera sits exactly at player's eye position
    /// No offset needed - camera is "inside" the player's head
    /// </summary>
    private float distanceFPP = 0f;

    // =====================================================
    //     PERSPECTIVE SWITCHING SYSTEM VARIABLES
    // =====================================================

    /// <summary>
    /// Target Z-distance for camera based on current perspective mode
    /// Recalculated every frame in SwitchPerspective()
    /// 
    /// Values:
    /// - When isTPP = false (FPP): targetDistance = 0f (camera at player position)
    /// - When isTPP = true (TPP): targetDistance = -2f (camera 2 units behind)
    /// 
    /// Note: Negative Z means "behind" the player in Unity's coordinate system
    /// (Forward = +Z, so Behind = -Z)
    /// </summary>
    private float targetDistance;

    /// <summary>
    /// The destination position we want the camera to reach
    /// Calculated each frame based on current perspective mode
    /// 
    /// Structure: Vector3(0, currentY, targetZ)
    /// - X = 0 (camera stays centered on player, no left/right offset)
    /// - Y = current local Y height (preserved from current position)
    /// - Z = target distance (0 for FPP, -distanceTPP for TPP)
    /// 
    /// This is fed into SmoothDamp as the "target" parameter
    /// </summary>
    private Vector3 targetPosition;

    /// <summary>
    /// The resulting smoothed position after SmoothDamp calculation
    /// This is the ACTUAL position we assign to transform.localPosition
    /// 
    /// SmoothDamp calculates an intermediate value between:
    /// - Current position (transform.localPosition)
    /// - Target position (targetPosition)
    /// 
    /// The result gradually approaches targetPosition over time,
    /// creating that nice smooth transition effect!
    /// </summary>
    private Vector3 smoothedPos;

    /// <summary>
    /// INTERNAL VELOCITY used by SmoothDamp function
    /// CRITICAL: Must be stored as a field (not local variable)!
    /// 
    /// What it does:
    /// - SmoothDamp uses this internally to track how fast camera is moving
    /// - Modified BY REFERENCE each frame (that's why we use 'ref' keyword)
    /// - Maintains continuity between frames (prevents jerky movement)
    /// 
    /// Why it must persist (field vs local variable):
    /// ❌ If declared inside SwitchPerspective(): Resets to zero every frame
    ///    → SmoothDamp always thinks velocity is 0 → Movement stutters!
    ///    
    /// ✅ As class field: Remembers velocity from previous frame
    ///    → SmoothDamp accelerates/decelerates naturally → Butter smooth! 🧈
    /// 
    /// You typically never need to read this value directly.
    /// Just pass it to SmoothDamp with 'ref' and let Unity handle it.
    /// </summary>
    private Vector3 smoothVelocity;

    /// <summary>
    /// Initialization - called once when the script first loads
    /// Sets up reference to the player body (parent transform)
    /// Converts distanceTPP to negative value (behind player = -Z in Unity)
    /// </summary>
    void Start()
    {
        // Get reference to parent transform (the player body/character controller)
        playerBody = transform.parent;

        // Log warning if no parent exists (camera won't rotate horizontally)
        if (!playerBody) Debug.Log("No player body found");

        // Convert TPP distance to negative
        // In Unity: Player faces +Z (forward), so "behind" is -Z
        // User enters "2" in Inspector (positive, intuitive)
        // We store as "-2" (correct direction for local positioning)
        distanceTPP = -distanceTPP; // 2f becomes -2f
    }

    /// <summary>
    /// Called every frame - handles input processing and rotation calculations
    /// Reads mouse input and calculates new rotation values with clamping
    /// Also updates perspective state and triggers camera position smoothing
    /// </summary>
    void Update()
    {
        // Get mouse input from centralized InputsManager singleton
        // Multiply by sensitivity for adjustable look speed
        // Multiply by Time.deltaTime for frame-rate independent movement
        mouseX = InputsManager.Instance.cameraInputs.x * sensitivity * Time.deltaTime;
        mouseY = InputsManager.Instance.cameraInputs.y * sensitivity * Time.deltaTime;

        // Calculate vertical rotation (looking up/down)
        // Subtract because mouse Y up should look up (negative Euler angle)
        xRotation -= mouseY;

        // Clamp vertical rotation to prevent over-rotation (looking behind)
        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);

        // Store horizontal rotation for LateUpdate
        // This will rotate the entire player body left/right
        yRotation = mouseX;
    }

    /// <summary>
    /// Called after all Update functions - applies the calculated rotations
    /// Using LateUpdate ensures all physics/movement updates happen first,
    /// preventing camera jitter or inconsistent behavior
    /// </summary>
    private void LateUpdate()
    {
        // Rotate player body around Y axis (horizontal/left-right looking)
        // Only if playerBody reference exists
        if (playerBody) playerBody.Rotate(Vector3.up * yRotation);

        // Apply vertical rotation to camera's local rotation
        // Using localRotation ensures it rotates relative to parent (player body)
        // X axis controls pitch (up/down), Y and Z remain at 0 to prevent tilting
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Note: Position is already updated in SwitchPerspective() during Update()
        // We don't need to set position here because:
        // 1. Position update doesn't depend on rotation
        // 2. Doing it in Update() ensures it runs before rendering
    }

    // =====================================================
    //              PERSPECTIVE SWITCHING SYSTEM
    // =====================================================

    /// <summary>
    /// Smoothly transitions camera position between First Person and Third Person modes.
    /// Uses Vector3.SmoothDamp for professional-quality smooth interpolation.
    ///
    /// SMOOTH DAMP vs LERP:
    /// - Lerp: Linear interpolation (constant speed)
    ///   Fast at start, same speed throughout, abrupt stop ❌
    ///   
    /// - SmoothDamp: Spring-damper physics simulation
    ///   Starts slow → speeds up → slows down → gentle stop ✅✅
    ///   Feels more natural and "professional"
    /// 
    /// CALLED FROM: Update() every frame
    /// WHY: Needs to run every frame to continuously animate toward target
    /// </summary>
    private void SwitchPerspective()
    {
        // ==================================================================
        // STEP 1: Determine target distance based on current perspective mode
        // ==================================================================

        /// Ternary operator (shorthand if-else):
        /// condition ? value_if_true : value_if_false
        /// 
        /// If isTPP is true:  targetDistance = distanceTPP (-2f) → Pull camera back
        /// If isTPP is false: targetDistance = distanceFPP (0f)  → Camera at player

        targetDistance = isTPP ? distanceTPP : distanceFPP;

        // Example outputs:
        // isTPP=true  → targetDistance = -2f (behind player)
        // isTPP=false → targetDistance = 0f  (at player position)

        // ==================================================================
        // STEP 2: Construct target position vector
        // ==================================================================

        /// Build the destination Vector3 for SmoothDamp
        /// 
        /// Parameters:
        /// - X = 0f: Camera stays horizontally centered on player
        ///          (No left/right offset - camera looks straight ahead)
        ///          
        /// - Y = transform.localPosition.y: Preserve current height!
        ///          We DON'T change Y here because height is controlled
        ///          separately (could add jumping/crouching height changes later)
        ///          By using current Y, we maintain whatever height the camera has
        ///          
        /// - Z = targetDistance: The calculated distance from Step 1
        ///          0f for FPP (camera at player origin)
        ///          -2f for TPP (camera pulled behind player)

        targetPosition.Set(0f, transform.localPosition.y, targetDistance);

        // Visual representation of target positions:
        //
        // FPP Target: (0, 0.4, 0)    ← Inside player's head
        // TPP Target: (0, 0.4, -2)   ← 2 units behind player

        // ==================================================================
        // STEP 3: Calculate smoothed position using SmoothDamp
        // ==================================================================

        /// Vector3.SmoothDamp()
        /// 
        /// Signature:
        /// SmoothDamp(current, target, ref velocity, smoothTime)
        /// 
        /// Parameters:
        /// ─────────────────────────────────────────────────────────────
        /// PARAMETER 1: transform.localPosition
        ///   - Current actual position of the camera (right now)
        ///   - Where the camera IS this frame
        ///   
        /// PARAMETER 2: targetPosition  
        ///   - Where we WANT the camera to be
        ///   - Calculated in Step 2 above
        ///   
        /// PARAMETER 3: ref smoothVelocity
        ///   - 'ref' means SmoothDamp can MODIFY this variable
        ///   - SmoothDamp reads AND writes to this each frame
        ///   - Stores internal velocity for physics-based smoothing
        ///   - MUST be a persistent field (not local variable)!
        ///   
        /// PARAMETER 4: smoothTime * Time.deltaTime
        ///   - smoothTime: Base smooth time from Inspector (0.25 seconds)
        ///   - Time.deltaTime: Time since last frame (~0.016s at 60fps)
        ///   - Multiplied together: Frame-rate independent smoothing

        smoothedPos = Vector3.SmoothDamp(
            transform.localPosition,     // Current position (start point)
            targetPosition,             // Target position (destination)
            ref smoothVelocity,         // Velocity tracker (modified by function!)
            smoothTime * Time.deltaTime // Smoothing factor (controls speed)
        );

        // ==================================================================
        // STEP 4: Apply the smoothed position to the camera
        // ==================================================================

        /// Assign the calculated smoothed position to the camera's local position
        /// 
        /// Using localPosition (NOT world position) because:
        /// - Camera is a CHILD of the player body
        /// - localPosition = relative to parent (player)
        /// - worldPosition = absolute position in scene
        /// 
        /// If we used position instead:
        /// - Camera would jump to world coordinates
        /// - Wouldn't follow player when they move!
        /// 
        /// With localPosition:
        /// - Camera maintains offset from player
        /// - Automatically follows player movement
        /// - Perfect for both FPP and TPP views

        transform.localPosition = smoothedPos;
    }
}