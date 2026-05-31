using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonTransition : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator m_Animator;

    public string parameterName = "IsHovering";

    private int parameterHash;

    void Start()
    {
        parameterHash = Animator.StringToHash(parameterName);
        m_Animator = GetComponent<Animator>();
        if (!m_Animator) Debug.Log("Animator not found");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_Animator.SetBool(parameterHash, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_Animator.SetBool(parameterHash, false);
    }
}
