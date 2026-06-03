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

    //Loading slider to check the progress of loading of scene
    private Slider loadingSlider;

    //Text to see the percentage of loading or progress
    private TextMeshProUGUI progressText;

    //Addition variable to store game time when the game runs
    private float timer;

    //Stores the progress of loading of the scene 
    float progress;

    //Async variable to load the scene in background of the opened scene
    private AsyncOperation operation;

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

        //Fetches the slider component or loading bar from the loading screen
        loadingSlider = loadingScreen.GetComponentInChildren<Slider>();

        //Fetches the Text or TMP component from the loading screen
        progressText = loadingScreen.GetComponentInChildren<TextMeshProUGUI>();
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

            //Null checks for slider 
            if(loadingSlider != null)
            {
                //stores the value of slider to its target position along with time when the game starts
                progress = Mathf.MoveTowards(loadingSlider.value, Mathf.Clamp01(operation.progress / 0.9f), Time.deltaTime);
                
                //Sets the slider value as the value updates
                loadingSlider.value = progress;
            }

            //Null checks for text or TMP
            if(progressText)
            {
                //Sets the value of progress into string
                //F0: No numbers shown after the decimal point begins
                progressText.text = $"{progress * 100:F0}%";
            }

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
