using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    //Maximum shooting range of the gun
    [SerializeField] private float m_Range = 50f;
    //Amount of the damage dealt by the gun when hitting target
    [SerializeField] private float m_Damage = 10f;
    //Amount of impact when target get damaged
    [SerializeField] private float m_ImpactForce = 100f;
    //Image reference to the crosshair
    [SerializeField] private Image m_Crosshair;
    
    //FPS camera reference
    private Camera m_FPSCam;

    //Delay between each bullet fired
    private float m_RecoilDuration = 0.5f;

    //TARGET script reference
    private Target m_TargetObject;

    //private variables to help 
    private float m_timer;

    private RaycastHit m_Hit;
    private RaycastHit m_IsTarget;

    private void Start()
    {
        //Assign the main camera 
        m_FPSCam = Camera.main;

        //sends log to console if no crosshairs assigned
        if (!m_Crosshair) Debug.Log("No Crosshair");
    }

    private void Update()
    {
        //Stores the time from since the game starts
        m_timer += Time.deltaTime;

        //Fire bullet when "Left Mouse Button" pressed and when the timer is greater than recoil duration
        if (InputsManager.Instance.attackState && m_timer > m_RecoilDuration)
        {
            //Reset the timer
            m_timer = 0;
            //Method to fire bullets
            ShootBullet();
        }
        //Method to check if the target is within the shooting range
        GunShootingRange();
    }

    /// <summary>
    /// Shoots bullet when the target is within the range 
    /// </summary>
    void ShootBullet()
    {
        //Shoots a raycast from the FPS camera to the range of the gun
        if (Physics.Raycast(m_FPSCam.transform.position, m_FPSCam.transform.forward, out m_Hit, m_Range))
        {
            //Fetches the "TARGET" component from the hitted object 
            m_TargetObject = m_Hit.transform.GetComponent<Target>();

            //Checks if there is rigidbody is attched to the target
            if (m_Hit.rigidbody)
            {
                //Adds an negative force to the normals of the target object
                m_Hit.rigidbody.AddForce(-m_Hit.normal * m_ImpactForce);
            }

            //Checks if there is "TARGET" component attched to object which get hitted by raycast
            if (m_TargetObject)
            {
                //Gives damage to the target object
                m_TargetObject.TakeDamage(m_Damage);
            }


        }
    }

    /// <summary>
    /// Max range for shooting the target object from the gun
    /// </summary>
    void GunShootingRange()
    {
        //Shoots a raycast from the FPS camera to the range of the gun
        if (Physics.Raycast(m_FPSCam.transform.position, m_FPSCam.transform.forward, out m_IsTarget, m_Range))
        {
            //Checks the object tag which got hitted by the raycast
            if (m_IsTarget.transform.CompareTag("Target"))
                //Change the crosshair color from white to red
                m_Crosshair.color = Color.red;
            else
                //Reset the crosshair color from red to white
                m_Crosshair.color = Color.white;
        }
    }
}
