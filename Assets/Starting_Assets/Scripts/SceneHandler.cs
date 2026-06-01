using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneHandler : MonoBehaviour
{
    [Header("Load Settings")]
    [Tooltip("Minimum loading time requried to load next scene or new scene")]
    public float minLoadTimer = 2f;

    private Slider loadingSlider;
    private TextMeshProUGUI progressText;

    private float timer;
    float progress;
    private AsyncOperation operation;


    public void StartSceneLoader(string sceneName)
    {
        StartCoroutine(LoadScene(sceneName));
    }

    public void ActivateLoadingScreen(GameObject loadingScreen)
    {
        loadingScreen.SetActive(true);
        loadingSlider = loadingScreen.GetComponentInChildren<Slider>();
        progressText = loadingScreen.GetComponentInChildren<TextMeshProUGUI>();
    }

    IEnumerator LoadScene(string sceneName)
    {
        timer = 0f;

        operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while(true)
        {
            timer += Time.deltaTime;

            if(loadingSlider != null)
            {
                progress = Mathf.MoveTowards(loadingSlider.value, Mathf.Clamp01(operation.progress / 0.9f), Time.deltaTime);
                loadingSlider.value = progress;
            }

            if(progressText)
            {
                progressText.text = $"{progress * 100:F0}%";
            }

            if(operation.progress >= 0.9f && timer > minLoadTimer)
            {
                operation.allowSceneActivation = true;
                yield break;
            }
            yield return null;
        }
    }

}
