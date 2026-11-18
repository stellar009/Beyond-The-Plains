using Unity.Cinemachine;
using UnityEngine;
using BeyondThePlains.InputSystem;

namespace BeyondThePlains.Player
{
    public partial class Player : MonoBehaviour
    {
        [Header("Player Settings")]
        public float walkSpeed = 2f;
        public float runSpeed = 8f;
        public float rotationSpeed = 5f;
        public float gravity = -9.81f;
        public bool allowOnlyWalk = false;

        [Header("Player Animation Settings")]
        public string animationBlendName = "Speed";
        [Range(0.1f, 1f)] public float animationSmoothTime = 0.2f;

        [Header("Camera Animation Settings")]
        public Animator cameraAnimator;
        public bool allowOnlyWalkCamAngle = false;
        public string cameraAnimBlendName = "ChangeAngle";
        [Range(0.1f, 1f)] public float cameraAnimationSmoothTime = 0.2f;

        private GameInputManager m_GameInputs;
        private CharacterController m_PlayerCharacterController;
        private CinemachineCamera m_Camera;
        private Animator m_Animator;

        private Vector3 m_Movement;
        private Vector3 m_MovementDirection;
        private Vector3 m_CamForward;
        private Vector3 m_CamRight;
        private Quaternion m_TargetRotation;
        private Vector3 m_MovementDir;

        private float speed;
        private bool isMoving;

        private int _AnimationBlendHash;
        private int _CameraAnimBlendHash;


        private enum _PlayerState { idle, walk, run }
        _PlayerState state = _PlayerState.idle;

        void Start()
        {
            Fetch();

            gravity = gravity > 0 ? -gravity : gravity;

            _AnimationBlendHash = Animator.StringToHash(animationBlendName);
            _CameraAnimBlendHash = Animator.StringToHash(cameraAnimBlendName);

            speed = walkSpeed;

            if (allowOnlyWalkCamAngle) SwitchCamAngle(1);
            if (allowOnlyWalk) m_GameInputs.EnableSprint(!allowOnlyWalk);
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

            if (m_MovementDirection.sqrMagnitude >= 0.0001)
            {
                m_TargetRotation = Quaternion.LookRotation(m_MovementDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, m_TargetRotation, rotationSpeed);
            }

            m_Movement = new Vector3(m_MovementDirection.x, m_Movement.y, m_MovementDirection.z);

            m_PlayerCharacterController.Move(m_Movement * Time.deltaTime * speed);
        }

        void OnGround()
        {
            if (m_PlayerCharacterController.isGrounded)
            {
                m_Movement.y = -2f;
            }
            else
            {
                m_Movement.y += gravity * Time.deltaTime;
            }
        }

        void Fetch()
        {
            m_PlayerCharacterController = GetComponent<CharacterController>();
            m_Animator = GetComponentInChildren<Animator>();

            m_GameInputs = FindObjectOfType<GameInputManager>();
            m_Camera = FindObjectOfType<CinemachineCamera>();

            if (!m_Camera) Debug.Log("No camera");
            if (!m_GameInputs) Debug.Log("No Inputs");
            if (!cameraAnimator) Debug.Log("No Camera Animator");
        }
    }
}
