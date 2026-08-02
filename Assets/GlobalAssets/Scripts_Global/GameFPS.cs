using System.Collections;
using TMPro;
using UnityEngine;

public class GameFPS : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField][Range(1f, 5f)] private float m_UpdateInterval = 2f;

    private TextMeshProUGUI m_FPSText;
    private float m_FrameRate;
    private WaitForSeconds m_CoroutineDelay;

    private void Start()
    {
        m_FPSText = GetComponent<TextMeshProUGUI>();

        m_CoroutineDelay = new WaitForSeconds(m_UpdateInterval);
        StartCoroutine(CalculateFPS());
    }

    private void Update()
    {
        m_FrameRate = 1/Time.unscaledDeltaTime;
    }

    IEnumerator CalculateFPS()
    {
        while(true)
        {
            m_FPSText.text = $"{m_FrameRate:F0} FPS";
            yield return m_CoroutineDelay;
        }
    }
}
