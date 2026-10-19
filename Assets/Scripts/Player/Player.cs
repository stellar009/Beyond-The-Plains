using Unity.Cinemachine;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 8f;
    public float rotationSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Animation Settings")]
    public string animationBlendName = "Speed";
    public float animationSmoothTime = 0.2f;

    private GameInputManager m_GameInputs;
    private CharacterController m_PlayerCharacterController;
    private CinemachineCamera m_Camera;
    private Animator m_Animator;


    private Vector3 m_Movement;
    private Vector3 m_MovementDirection;
    private Vector3 m_CamForward;
    private Vector3 m_CamRight;
    private Quaternion m_TargetRotation;
    private Vector3 moveDirMagnitude;
    public float speed;

    private int _AnimationBlendHash;
    private enum _AnimState { idle, walk, running}
    private _AnimState currentState = _AnimState.idle;

    void Start()
    {
        m_GameInputs = FindObjectOfType<GameInputManager>();
        m_PlayerCharacterController = GetComponent<CharacterController>();
        m_Camera = FindObjectOfType<CinemachineCamera>();
        m_Animator = GetComponentInChildren<Animator>();

        if (!m_Camera) Debug.Log("No camera");
        if (!m_GameInputs) Debug.Log("No Inputs");

        gravity = gravity > 0 ? -gravity : gravity;

        _AnimationBlendHash = Animator.StringToHash(animationBlendName);

        speed = walkSpeed;
    }

    void Update()
    {
        Movement(); 
        HandleAnimations();
    }

    void Movement()
    {
        OnGround();
        m_Movement.x = m_GameInputs.playerMovement.x;
        m_Movement.z = m_GameInputs.playerMovement.y;

        m_CamForward = m_Camera.transform.forward;
        m_CamRight = m_Camera.transform.right;

        m_CamForward.y = 0f;
        m_CamRight.y = 0f;

        m_CamForward.Normalize();
        m_CamRight.Normalize();

        m_MovementDirection = m_CamRight * m_Movement.x + m_CamForward * m_Movement.z;

        if(m_MovementDirection.sqrMagnitude >= 0.0001)
        {
            m_TargetRotation = Quaternion.LookRotation(m_MovementDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, m_TargetRotation, rotationSpeed);
        }

        m_Movement = new Vector3(m_MovementDirection.x, m_Movement.y, m_MovementDirection.z);

        m_PlayerCharacterController.Move(m_Movement * Time.deltaTime * speed);
    }

    void OnGround()
    {
        if(m_PlayerCharacterController.isGrounded)
        {
            m_Movement.y = -2f;
        }
        else
        {
            m_Movement.y += gravity * Time.deltaTime;
        }
    }

    void HandleAnimations()
    {
        moveDirMagnitude = new Vector3(m_MovementDirection.x, 0f, m_MovementDirection.z);

        if(moveDirMagnitude.magnitude > 0.01f && m_GameInputs.isSprint)
        {
            speed = runSpeed;
            PerformLocomotionAnimation(1);
        }
        else if(moveDirMagnitude.magnitude > 0.01f && !m_GameInputs.isSprint)
        {
            speed = walkSpeed;
            PerformLocomotionAnimation(0.5f);
        }
        else
        {
            PerformLocomotionAnimation(0);
        }
    }

    void PerformLocomotionAnimation(float value)
    {
        m_Animator.SetFloat(_AnimationBlendHash, value, animationSmoothTime, Time.deltaTime);
    }
}
