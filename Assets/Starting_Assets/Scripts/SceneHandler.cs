using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneHandler : MonoBehaviour
{
    [Header("Load Settings")]
    [Tooltip("Minimum loading time requried to load next scene or new scene")]
    //Minimum loading time used to load a scene in background
    public float minLoadTimer = 2f;

    //Cache memory or variables

    //Addition variable to store game time when the game runs
    private float timer;

    //Async variable to load the scene in background of the opened scene
    private AsyncOperation operation;

    //Text used on the button
    private TextMeshProUGUI btnText;

    private void Start()
    {
        //Fetches the component from the children of the parent object
        btnText = GetComponentInChildren<TextMeshProUGUI>();

        if (btnText == null) return; //returns null value when the btnText not found 
        else
            btnText.enabled = true; //Ensures the text is enabled when the game starts
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
    /// Activates the loading screen when needed or can be used via other scripts
    /// </summary>
    /// <param name="loadingScreen"></param>
    public void ActivateLoadingScreen(GameObject loadingScreen)
    {
        //Activate the loading screen 
        loadingScreen.SetActive(true);
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
        timer = 0f;

        //Starts Loading the scene in background 
        operation = SceneManager.LoadSceneAsync(sceneName);

        //Disables to open the scene when it loaded 
        operation.allowSceneActivation = false;

        while(true)
        {
            //Stores the time when the game runs
            timer += Time.deltaTime;

            //Checks the progress is completed more than 90% 
            //Checks the timer is greater than the minimum loading time to load a scene
            if(operation.progress >= 0.9f && timer > minLoadTimer)
            {
                //Allows the scene to open or activate from the RAM or GPU memory
                operation.allowSceneActivation = true;

                //Breaks the condition
                yield break;
            }
            //Wait for a frame to complete 
            yield return null;
        }
    }

}
