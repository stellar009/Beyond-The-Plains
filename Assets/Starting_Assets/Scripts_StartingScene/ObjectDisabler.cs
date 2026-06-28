using UnityEngine;

public class ObjectDisabler : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Add only objects which needs to disable when game starts")]
    public GameObject[] objects;

    private int m_ObjectsSize = 0;

    private void Start()
    {
        m_ObjectsSize = objects.Length;

        for (int i = 0; i < m_ObjectsSize; i++)
        {
            if (!objects[i].activeInHierarchy || !objects[i])
            {
                i++;
                return;
            }
            else
            {
                objects[i].SetActive(false);
            }
        }
    }
}
