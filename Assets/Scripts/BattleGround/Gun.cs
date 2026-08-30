using UnityEngine;

public class Gun : MonoBehaviour
{

    [SerializeField] private int m_MaxBulletCount = 20;
    [SerializeField] private GameObject m_BulletPrefab;

    private int count = 1;
    private float delay = 0.5f;
    private float timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!m_BulletPrefab) Debug.Log("No bullets");
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(InputsManager.Instance.attackState && count <= m_MaxBulletCount && timer > delay)
        {
            timer = 0;
            FireBullet();
            count++;
            Debug.Log($"Bullets fired: {count}");
        }
    }

    void FireBullet()
    {
        Instantiate(m_BulletPrefab, gameObject.transform);
    }
}
