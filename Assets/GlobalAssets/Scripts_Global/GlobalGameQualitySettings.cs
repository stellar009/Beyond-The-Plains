using UnityEngine;
using UnityEngine.Rendering;

public class GlobalGameQualitySettings : MonoBehaviour
{
    private void Start()
    {
        EnableSRPBatching(true);
        Application.targetFrameRate = 60;
    }

    void EnableSRPBatching(bool state)
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = state;
    }
}