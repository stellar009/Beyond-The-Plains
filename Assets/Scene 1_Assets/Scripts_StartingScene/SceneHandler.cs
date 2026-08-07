using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    [Header("Load Settings")]
    [Tooltip("Minimum loading time requried to load next scene or new scene")]
    //Minimum loading time used to load a scene in background
    public float minLoadTimer = 2f;

    //Cache memory or variables

    //Addition variable to store game time when the game runs
    private float m_Timer;

    //Async variable to load the scene in background of the opened scene
    private AsyncOperation m_Operation;

    //Text used on the button
    private TextMeshProUGUI m_BtnText;

    private void Start()
    {
        //Fetches the component from the children of the parent object
        m_BtnText = GetComponentInChildren<TextMeshProUGUI>();

        if (m_BtnText == null) return; //returns null value when the btnText not found 
        else
            m_BtnText.enabled = true; //Ensures the text is enabled when the game starts
    }

    /// <summary>
    /// PUBLIC method used to load the scene via other scipts to load the scene 
    /// </summary>
    /// <param name="sceneName">Requires a string or name of the scene to load</param>
    public void StartSceneLoader(string sceneName)
    {
        //Starts the coroutine to load the scene in background
        StartCoroutine(LoadScene(sceneName));
    }

    /// <summary>
    /// Coroutine to load the scene in background 
    /// Takes the scene name as parameter to load in the background
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    IEnumerator LoadScene(string sceneName)
    {
        //Default timer to store game time
        m_Timer = 0f;

        //Starts Loading the scene in background 
        m_Operation = SceneManager.LoadSceneAsync(sceneName);

        //Disables to open the scene when it loaded 
        m_Operation.allowSceneActivation = false;

        while(true)
        {
            //Stores the time when the game runs
            m_Timer += Time.deltaTime;

            //Checks the progress is completed more than 90% 
            //Checks the timer is greater than the minimum loading time to load a scene
            if(m_Operation.progress >= 0.9f && m_Timer > minLoadTimer)
            {
                //Allows the scene to open or activate from the RAM or GPU memory
                m_Operation.allowSceneActivation = true;

                //Breaks the condition
                yield break;
            }
            //Wait for a frame to complete 
            yield return null;
        }
    }
}
