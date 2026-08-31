using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    public float health = 10f;

    private Renderer m_Renderer;

    private void Start()
    {
        m_Renderer = GetComponent<Renderer>();
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        m_Renderer.material.color = Color.antiqueWhite;

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        gameObject.SetActive(false);
    }
}
