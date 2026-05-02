using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float sensitivity = 10f;
    public float maxLookAngle = 90f;
    public float minLookAngle = -60f;

    private Transform playerBody;
    private float xRotation = 0f;
    private float yRotation = 0f;

    float mouseX, mouseY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerBody = transform.parent;
        if (!playerBody) Debug.Log("No player body found");
    }

    // Update is called once per frame
    void Update()
    {
        mouseX = InputsManager.Instance.cameraInputs.x * sensitivity * Time.deltaTime;
        mouseY = InputsManager.Instance.cameraInputs.y * sensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);

        yRotation = mouseX;
    }

    private void LateUpdate()
    {
        if (playerBody) playerBody.Rotate(Vector3.up * yRotation);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
