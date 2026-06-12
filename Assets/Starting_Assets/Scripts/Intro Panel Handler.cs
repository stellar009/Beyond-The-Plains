using System.Collections;
using UnityEngine;

public class IntroPanelHandler : MonoBehaviour
{
    //settings to set up Intro panel behaviour
    [Header("Settings")]
    public GameObject[] panels; //Array to store intro panels 
    public float delay = 2f; //Global delay for all panels in the array 

    //cache memory for runtime optimization
    private int panelSize = 0; //Max size of an array
    private WaitForSeconds routineDelay; //Coroutine WaitForSeconds cache

    //Runs first when the game starts
    private void Awake()
    {
        //sets the panel size when the game starts
        panelSize = panels.Length;

        //Loop through all panels and turns them off
        for(int i = 0; i < panelSize; i++)
        {
            //turns off all panels in the array 
            panels[i].SetActive(false);
        }
    }

    //Runs after Awake 
    private void Start()
    {
        //Caches the WaitForSeconds 
        routineDelay = new WaitForSeconds(delay);

        //Starts the coroutine
        StartCoroutine(PanelRoutine());
    }

    /// <summary>
    /// Handles when which panel needs to activate with an timer
    /// </summary>
    /// <returns></returns>
    IEnumerator PanelRoutine()
    {
        //loops throught all array elements 
        for(int i = 0;i < panelSize;i++)
        {
            //Activates the panels 
            panels[i].SetActive(true);

            //Delay for routine the next part runs when the delay is completed
            yield return routineDelay;

            //Checks if the panel is active 
            if (panels[i].active)
            {
                //If panel is active disable it
                panels[i].SetActive (false);
            }
        }
    }
}
