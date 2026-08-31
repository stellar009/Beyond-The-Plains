using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    [SerializeField] private float m_Range = 50f;
    [SerializeField] private float m_Damage = 10f;
    [SerializeField] private float m_ImpactForce = 100f;
    [SerializeField] private Image m_Crosshair;

    private Camera m_FPSCam;

    private float m_RecoilDuration = 0.5f;
    private float m_timer;

    private Target m_TargetObject;
    private RaycastHit m_Hit;
    private RaycastHit m_IsTarget;

    private void Start()
    {
        m_FPSCam = Camera.main;

        if (!m_Crosshair) Debug.Log("No Crosshair");
    }

    private void Update()
    {
        m_timer += Time.deltaTime;

        if (InputsManager.Instance.attackState && m_timer > m_RecoilDuration)
        {
            m_timer = 0;
            ShootBullet();
        }
        GunRange();
    }

    void ShootBullet()
    {
        if (Physics.Raycast(m_FPSCam.transform.position, m_FPSCam.transform.forward, out m_Hit, m_Range))
        {
            m_TargetObject = m_Hit.transform.GetComponent<Target>();

            if (m_Hit.rigidbody)
            {
                m_Hit.rigidbody.AddForce(-m_Hit.normal * m_ImpactForce);
            }

            if (m_TargetObject)
            {
                m_TargetObject.TakeDamage(m_Damage);
            }


        }
    }

    void GunRange()
    {
        if (Physics.Raycast(m_FPSCam.transform.position, m_FPSCam.transform.forward, out m_IsTarget, m_Range))
        {
            if (m_IsTarget.transform.CompareTag("Target"))
                m_Crosshair.color = Color.red;
            else
                m_Crosshair.color = Color.white;
        }
    }
}
