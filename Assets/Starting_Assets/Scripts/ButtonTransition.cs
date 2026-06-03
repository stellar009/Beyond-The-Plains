using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonTransition : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    ///<summary>
    ///Button animator to handle and perform animations for button interactions
    ///Interactions such as Hovering, Clicks, etc.
    ///</summary>
    private Animator m_Animator;

    /// <summary>
    /// Animation name which is need to perform 
    /// </summary>
    public string hoverAnimationName = "IsHovering";

    /// <summary>
    /// Hash value to create a unique integer to handle animations
    /// </summary>
    private int hoverAnimationHash;

    void Start()
    {
        //Converts string into unique Hash number
        hoverAnimationHash = Animator.StringToHash(hoverAnimationName);

        //Fetches animator component from the game object
        m_Animator = GetComponent<Animator>();

        //Null checks for animator 
        if (!m_Animator) Debug.Log("Animator not found");
    }

    /// <summary>
    /// OnPointerEnter handles the event performed when the mouse pointer enters 
    /// Event: When mouse pointer ENTERS on a button or an game object
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        //Sets the animation boolen TRUE to switch between animation states
        m_Animator.SetBool(hoverAnimationHash, true);
    }

    /// <summary>
    /// OnPointerExit handles the event performed when the mouse pointer exits 
    /// Event: When mouse pointer EXITS on a button or an game object
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        //Sets the animation boolen FALSE to switch between animation states
        m_Animator.SetBool(hoverAnimationHash, false);
    }
}
