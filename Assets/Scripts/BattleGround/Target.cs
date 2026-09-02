using UnityEngine;

public class Target : MonoBehaviour
{
    //Health of the target object
    public float health = 10f;

    //Renderer of the target object
    private Renderer m_Renderer;

    private void Start()
    {
        //Fetch the renderer component form the game object
        m_Renderer = GetComponent<Renderer>();
    }

    /// <summary>
    /// Public function for other scripts used to damage the target object
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(float damage)
    {
        //Reduces the health given by the other scripts like gun, etc.
        health -= damage;

        //Change the color of the target when it takes damage
        m_Renderer.material.color = Color.antiqueWhite;

        //If condition when the target object health is reduced to Zero
        if (health <= 0)
        {
            //Function or method performed when the health of the object is zero
            Die();
        }
    }

    /// <summary>
    /// Disables the game object 
    /// </summary>
    void Die()
    {
        gameObject.SetActive(false);
    }
}
