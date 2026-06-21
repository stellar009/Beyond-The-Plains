using TMPro;
using UnityEngine;

public class ClockTime : MonoBehaviour
{
    private TextMeshProUGUI m_ClockText;

    private float m_GameSeconds;

    private int m_GameMinutes;
    private int m_GameHours;
    private int m_TotalGameMinutes;

    private void Start()
    {
        m_ClockText = GetComponent<TextMeshProUGUI>();
        if (!m_ClockText) Debug.Log($"{m_ClockText} Component Not Found");
    }

    private void Update()
    {
        CalculateTime();
    }

    void CalculateTime()
    {
        m_GameSeconds += Time.deltaTime * 60f;
        m_TotalGameMinutes = Mathf.FloorToInt(m_GameSeconds / 60f);

        if ((m_TotalGameMinutes / 60) < 24)
        {
            m_GameHours = m_TotalGameMinutes / 60;
            m_GameMinutes = m_TotalGameMinutes % 60;
        }
        else
        {
            m_GameSeconds = 0;
            m_TotalGameMinutes = 0;
        }
        
        m_ClockText.text = $"{m_GameHours} : {m_GameMinutes}";
    }
}
