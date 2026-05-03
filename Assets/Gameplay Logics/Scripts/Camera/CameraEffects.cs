using System.Collections;
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    private Transform mainCameraTransform;

    [Header("Breathing Settings")]
    [Range(0.2f, 0.4f)] public float minBreathingIntensity = 0.3f;
    [Range(0.5f, 0.7f)] public float maxBreathingIntensity = 0.6f;
    public float breathingDelay = 1f;

    [Header("Walk Settings")]
    [Range(0.2f, 0.5f)]public float minWalkIntensity = 0.4f;
    [Range(0.6f, 0.8f)] public float maxWalkIntensity = 0.6f;
    public float walkEffectDelay = 1f;

    private WaitForSeconds breathDelay;
    private WaitForSeconds walkDelay;

    void Start()
    {
        mainCameraTransform = Camera.main.transform;
        if (!mainCameraTransform) Debug.Log("No camera Assigned ", this);

        breathDelay = new WaitForSeconds(breathingDelay);
        walkDelay = new WaitForSeconds(walkEffectDelay);
    }

    void CameraShakeWhileBreathing()
    {
        mainCameraTransform.localPosition = new Vector3(0f, Mathf.Lerp(minBreathingIntensity, maxBreathingIntensity, breathingDelay), 0f);
    }

    void CameraShakeWhileWalking()
    {
        mainCameraTransform.localPosition = new Vector3(0f, Random.Range(minWalkIntensity, maxWalkIntensity), 0f);
    }

    public IEnumerator Breathing()
    {
        while(true)
        {
            CameraShakeWhileBreathing();
            yield return breathDelay;
        }
    }

    public IEnumerator Walking()
    {
        while (true)
        {
            CameraShakeWhileWalking();
            yield return walkDelay;
        }
    }
}