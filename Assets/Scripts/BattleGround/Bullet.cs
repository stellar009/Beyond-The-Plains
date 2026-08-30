using UnityEngine;

public class Bullet : MonoBehaviour
{

    public float speed = 10f;

    private float m_BulletDuration = 5f;
    private float m_Timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        m_Timer += Time.deltaTime;

        transform.Translate(Vector3.up * Time.deltaTime * speed);

        if(m_Timer > m_BulletDuration)
        {
            DisableBullet();
        }
    }

    void DisableBullet()
    {
        gameObject.SetActive(false);
    }
}
