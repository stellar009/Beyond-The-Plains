using UnityEngine;
using UnityEngine.Rendering;

public class GlobalGameQualitySettings : MonoBehaviour
{
    public static GlobalGameQualitySettings GGQSInstance;

    private void Awake()
    {
        if(GGQSInstance == null)
        {
            GGQSInstance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
    }
}
