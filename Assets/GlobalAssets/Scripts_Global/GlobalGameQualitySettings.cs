using UnityEngine;
using UnityEngine.Rendering;

public class GlobalGameQualitySettings : MonoBehaviour
{
    private void Start()
    {
        EnableSRPBatching(true);
        UseVSync();
    }

    void EnableSRPBatching(bool state)
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = state;
    }

    public void UseVSync()
    {
        QualitySettings.vSyncCount = 1;
    }
}