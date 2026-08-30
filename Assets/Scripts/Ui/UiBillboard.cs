using UnityEngine;

public class UiBillboard : MonoBehaviour
{
    private Camera m_MainCam;

    private Transform m_MainCamTransform;

    void Start()
    {
        m_MainCam = Camera.main;
        if (m_MainCam == null) Debug.Log("Main Camera not found");

        m_MainCamTransform = m_MainCam.transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + m_MainCamTransform.rotation * Vector3.forward);
    }
}
