using System.Collections;
using UnityEngine;

public class PanelHandler : MonoBehaviour
{
    [Header("Scene Objects")]
    public GameObject m_SceneObject;

    //settings to set up Intro panel behaviour
    [Header("Intro Settings")]
    public GameObject[] panels; //Array to store intro panels 
    public float delay = 2f; //Global delay for all panels in the array 

    //cache memory for runtime optimization
    private int m_PanelSize = 0; //Max size of an array
    private WaitForSeconds m_RoutineDelay; //Coroutine WaitForSeconds cache

    //Runs first when the game starts
    private void Awake()
    {
        //sets the panel size when the game starts
        m_PanelSize = panels.Length;

        //Loop through all panels and turns them off
        for(int i = 0; i < m_PanelSize; i++)
        {
            //turns off all panels in the array 
            panels[i].SetActive(false);
        }

        EnableSceneObject(false);
    }

    //Runs after Awake 
    private void Start()
    {
        //Caches the WaitForSeconds 
        m_RoutineDelay = new WaitForSeconds(delay);

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
        for(int i = 0;i < m_PanelSize;i++)
        {
            //Activates the panels 
            panels[i].SetActive(true);

            //Delay for routine the next part runs when the delay is completed
            yield return m_RoutineDelay;

            //Checks if the panel is active 
            if (panels[i].activeInHierarchy)
            {
                //If panel is active disable it
                panels[i].SetActive (false);

                if(i == m_PanelSize-1)
                {
                    EnableSceneObject(true);
                }
            }
        }
    }

    /// <summary>
    /// Enable and disable scene object
    /// </summary>
    /// <param name="state"></param>
    public void EnableSceneObject(bool state)
    {
        m_SceneObject.SetActive(state);
    }

    /// <summary>
    /// Activates the loading screen when needed or can be used via other scripts
    /// </summary>
    /// <param name="loadingScreen"></param>
    public void ActivateLoadingScreen(GameObject loadingScreen)
    {
        //Activate the loading screen 
        loadingScreen.SetActive(true);
    }
}
