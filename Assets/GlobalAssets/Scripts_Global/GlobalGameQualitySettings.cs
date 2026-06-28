using UnityEngine;
using UnityEngine.Rendering;

public class GlobalGameQualitySettings : MonoBehaviour
{
    private void Start()
    {
        EnableSRPBatching(true);
    }

    void EnableSRPBatching(bool state)
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = state;
    }
}