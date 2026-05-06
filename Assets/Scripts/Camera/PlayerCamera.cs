using UnityEngine;

/// <summary>
/// PlayerCamera - Handles first-person camera rotation and mouse look functionality.
/// Attached to the camera object which should be a child of the player body.
/// </summary>
public class PlayerCamera : MonoBehaviour
{
    [Header("Global Perspective Settings")]
    [Range(0.01f, 2f)] public float smoothTime = 0.25f;

    [Header("Third PersonPrespective Settings")]
    [Range(1f, 5f)]public float distanceTPP = 2f;

    [Header("Mouse Settings")]
    [Tooltip("Mouse sensitivity multiplier for camera rotation speed")]
    public float sensitivity = 10f;

    [Tooltip("Maximum upward look angle in degrees")]
    public float maxLookAngle = 90f;

    [Tooltip("Minimum downward look angle in degrees")]
    public float minLookAngle = -60f;

    private bool isTPP;

    // Reference to the player body transform (parent object)
    private Transform playerBody;

    // Current vertical rotation (up/down looking) - applied to camera locally
    private float xRotation = 0f;

    // Current horizontal rotation (left/right looking) - applied to player body
    private float yRotation = 0f;

    // Raw mouse input values from input manager
    private float mouseX, mouseY;

    private float distanceFPP = 0f;

    private float targetDistance;

    private Vector3 targetPosition;
    private Vector3 smoothedPos;
    private Vector3 smoothVelocity;

    /// <summary>
    /// Initialization - called once when the script first loads
    /// Sets up reference to the player body (parent transform)
    /// </summary>
    void Start()
    {
        // Get reference to parent transform (the player body/character controller)
        playerBody = transform.parent;

        // Log warning if no parent exists (camera won't rotate horizontally)
        if (!playerBody) Debug.Log("No player body found");

        distanceTPP = -distanceTPP;
    }

    /// <summary>
    /// Called every frame - handles input processing and rotation calculations
    /// Reads mouse input and calculates new rotation values with clamping
    /// </summary>
    void Update()
    {
        // Get mouse input from centralized InputsManager singleton
        // Multiply by sensitivity for adjustable look speed
        // Multiply by Time.deltaTime for frame-rate independent movement
        mouseX = InputsManager.Instance.cameraInputs.x * sensitivity * Time.deltaTime;
        mouseY = InputsManager.Instance.cameraInputs.y * sensitivity * Time.deltaTime;

        isTPP = InputsManager.Instance.changePerspective;

        // Calculate vertical rotation (looking up/down)
        // Subtract because mouse Y up should look up (negative Euler angle)
        xRotation -= mouseY;

        // Clamp vertical rotation to prevent over-rotation (looking behind)
        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);

        // Store horizontal rotation for LateUpdate
        // This will rotate the entire player body left/right
        yRotation = mouseX;

        SwitchPerspective();
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
    }

    private void SwitchPerspective()
    {
        targetDistance = isTPP ? distanceTPP : distanceFPP;
        targetPosition.Set(0f, transform.localPosition.y, targetDistance);

        smoothedPos = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref smoothVelocity, smoothTime * Time.deltaTime);
        transform.localPosition = smoothedPos;
    }
}