using UnityEngine;

public class ObjectDisabler : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Add only objects which needs to disable when game starts")]
    public GameObject[] objects;

    private int objectsSize = 0;

    private void Start()
    {
        objectsSize = objects.Length;

        for (int i = 0; i < objectsSize; i++)
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
