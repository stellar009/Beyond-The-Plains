using System.Collections;
using UnityEngine;

public class RandomTreePlacer : MonoBehaviour
{
    [Header("Tree Settings")]
    [Range(20, 120)]public int treeCount = 50;
    public GameObject treePrefab;

    [Header("Tree Setup Settings")]
    public int maxRange = 250;
    [Range(5, 20)]public int safeValue = 10;

    private Transform treePrefabTransform;
    private int count;
    private int safeRange;

    private void Start()
    {
        treePrefabTransform = treePrefab.transform;
        safeRange = maxRange - safeValue;

        if (maxRange <= 0) Debug.Log("Invalid range for trees ", this);
        if (!treePrefab) Debug.Log("Trees are not assigned in inspector ", this);

        StartCoroutine(PlaceTrees());
    }

    IEnumerator PlaceTrees()
    {
        while (count < treeCount)
        {
            float distance = Vector3.Distance(treePrefabTransform.position, this.transform.position);

            if (distance < maxRange)
            {
                Instantiate(treePrefab, new Vector3(Random.Range(-safeRange, safeRange), treePrefabTransform.position.y, Random.Range(-safeRange, safeRange)), Quaternion.identity);
                count++;
                Debug.Log($"Trees added: {count}");
            }

            if(count >= treeCount)
            {
                Debug.Log("Object Destroyed ", this.gameObject);
                Destroy(this.gameObject);
            }
            yield return new WaitForSeconds(0.05f);
        }
    }
}
