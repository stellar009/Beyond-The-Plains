using System.Collections;
using UnityEngine;

public class IntroPanelHandler : MonoBehaviour
{
    [Header("Settings")]
    public GameObject[] panels;
    public float delay = 2f;

    private int panelSize = 0;
    private WaitForSeconds routineDelay;

    private void Awake()
    {
        panelSize = panels.Length;

        for(int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(false);
        }
    }

    private void Start()
    {
        routineDelay = new WaitForSeconds(delay);

        StartCoroutine(PanelRoutine());
    }

    IEnumerator PanelRoutine()
    {
        for(int i = 0;i < panels.Length;i++)
        {
            panels[i].SetActive(true);
            yield return routineDelay;

            if (panels[i].active)
            {
                panels[i].SetActive (false);
            }
        }
    }
}
