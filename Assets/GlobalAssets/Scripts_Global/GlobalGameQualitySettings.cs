using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class GlobalGameQualitySettings : MonoBehaviour
{
    [Header("Shader Settings")]
    [SerializeField] private ShaderVariantCollection m_WarmUpShaders;
    [SerializeField] private TextMeshProUGUI m_LogText;

    [Header("Version Settings")]
    [SerializeField] private TextMeshProUGUI m_VersionText;

    private void Start()
    {
        EnableSRPBatching(true);
        Application.targetFrameRate = 60;
        CompileShaders();
        CheckVersion();
    }

    void EnableSRPBatching(bool state)
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = state;
    }

    void CompileShaders()
    {
        if (m_WarmUpShaders.isWarmedUp)
        {
            m_LogText.text = "Shader Already Compiled";
            return;
        }

        string m_SavedVersion = PlayerPrefs.GetString("ShaderCompiled", "");
        bool m_FirstLaunch = m_SavedVersion != Application.version;

        m_WarmUpShaders.WarmUp();

        PlayerPrefs.SetString("ShaderCompiled", Application.version);
        PlayerPrefs.Save();
    }

    void CheckVersion()
    {
        m_VersionText.text = $"Windows_{Application.version}";
    }
}