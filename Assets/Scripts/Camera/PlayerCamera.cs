using UnityEngine;

/// <summary>
/// PlayerCamera - Controls the camera for first-person and third-person views.
/// Attach this to the camera object (child of the player).
/// Handles mouse look and perspective switching.
/// </summary>
public class PlayerCamera : MonoBehaviour
{
    [Header("Mouse Settings")]
    [Tooltip("How sensitive the mouse is for looking around")]
    [SerializeField] private float m_Sensitivity = 10f;

    [Tooltip("How far up you can look (degrees)")]
    [SerializeField] private float m_MaxLookAngle = 90f;

    [Tooltip("How far down you can look (degrees)")]
    [SerializeField] private float m_MinLookAngle = -60f;

    // Reference to the player body (the parent object)
    private Transform m_PlayerBody;

    // Current up/down rotation (camera looks up/down)
    private float m_XRotation = 0f;

    // Current left/right rotation (player body rotates)
    private float m_YRotation = 0f;

    // Mouse movement values from input system
    private float m_MouseX, m_MouseY;

    private InputsManager m_InputManager;

    /// <summary>
    /// Called when the script starts - gets the player body reference.
    /// </summary>
    void Start()
    {
        // Get the parent object (the player character)
        m_PlayerBody = transform.parent;

        // Warn if there's no parent (camera won't rotate properly)
        if (!m_PlayerBody) Debug.Log("No player body found");
    }

    /// <summary>
    /// Called every frame - reads mouse input and calculates rotation.
    /// </summary>
    void Update()
    {
        // Get mouse movement from InputsManager
        // Multiply by sensitivity for faster/slower look speed
        // Multiply by Time.deltaTime to make it frame-rate independent
        m_MouseX = InputsManager.Instance.cameraInputs.x * m_Sensitivity * Time.deltaTime;
        m_MouseY = InputsManager.Instance.cameraInputs.y * m_Sensitivity * Time.deltaTime;

        // Calculate vertical rotation (looking up/down)
        // Subtract because moving mouse up should look up
        m_XRotation -= m_MouseY;

        // Prevent looking too far up or down (so you don't flip upside down)
        m_XRotation = Mathf.Clamp(m_XRotation, m_MinLookAngle, m_MaxLookAngle);

        // Store horizontal rotation to apply in LateUpdate
        m_YRotation = m_MouseX;
    }

    /// <summary>
    /// Called after Update - applies the rotations.
    /// Runs after movement so the camera doesn't jitter.
    /// </summary>
    private void LateUpdate()
    {
        // Rotate the player body left/right (horizontal look)
        if (m_PlayerBody) m_PlayerBody.Rotate(Vector3.up * m_YRotation);

        // Rotate the camera up/down (vertical look)
        // localRotation keeps it relative to the player body
        // X axis = pitch (up/down), Y and Z stay at 0 (no tilting)
        transform.localRotation = Quaternion.Euler(m_XRotation, 0f, 0f);
    }
}