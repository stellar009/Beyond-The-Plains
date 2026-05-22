using UnityEngine;

public class Wobble : MonoBehaviour
{
    public float recoverySpeed = 1f;
    public float wobbleVelocityX = 1f;
    public float wobbleVelocityZ = 1f;
    public float wobbleStrength = 1f;
    public float wobbleSpeed = 1f;

    float wave;
    float wobbleX;
    float wobbleZ;
    float wobbleForceX;
    float wobbleForceZ;
    float elapsedTime = 0.2f;

    private Renderer objectRenderer;

    Vector3 velocity;
    Vector3 previousPosition;
    Vector3 previousRotation;
    Vector3 rotationDelta;

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (!objectRenderer) Debug.Log("No Renderer");
    }

    private void Update()
    {
        wobbleVelocityX = Mathf.Lerp(wobbleVelocityX, 0f, recoverySpeed * Time.deltaTime);
        wobbleVelocityZ = Mathf.Lerp(wobbleVelocityZ, 0f, recoverySpeed * Time.deltaTime);

        wave = Mathf.PI * 2f * wobbleSpeed;
        wobbleX = wobbleVelocityX * Mathf.Sin(wave * elapsedTime);
        wobbleZ = wobbleVelocityZ * Mathf.Sin(wave * elapsedTime);

        previousPosition = transform.position;
        velocity = (transform.position - previousPosition) / Time.deltaTime;

        previousRotation = transform.rotation.eulerAngles;
        rotationDelta = (transform.rotation.eulerAngles - previousRotation) / Time.deltaTime;

        wobbleForceX = velocity.x + rotationDelta.z * 0.2f;
        wobbleForceZ = velocity.z + rotationDelta.x * 0.2f;

        wobbleVelocityX += Mathf.Clamp(wobbleForceX * wobbleStrength, -wobbleStrength, wobbleStrength);
        wobbleVelocityZ += Mathf.Clamp(wobbleForceZ * wobbleStrength, -wobbleStrength, wobbleStrength);

        objectRenderer.material.SetFloat("_WobbleX", wobbleX);
        objectRenderer.material.SetFloat("_WobbleZ", wobbleZ);
    }
}
