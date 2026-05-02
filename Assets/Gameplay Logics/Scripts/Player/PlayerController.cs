using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private Animator characterAnimator;

    [Header("Player Settings")]
    public float speed = 2f;

    [Header("Gravity Settings")]
    [Tooltip("Gravity can be entered positive too")]
    public float gravity = 9.81f;

    [Header("Animation Settings")]
    public string animationBlendName = "Speed";
    public float animationSmoothTime = 0.2f;

    private Vector3 velocity;
    private Vector3 moveDirection;
    private Vector3 finalMovement;

    private float defaultSpeed = 2f;
    private float defaultGravity = -2f;

    private int animationBlendHash;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (!characterController) Debug.Log("No character Found");

        characterAnimator = GetComponentInChildren<Animator>();
        if (!characterAnimator) Debug.Log("No Animators");
    }

    private void Start()
    {
        animationBlendHash = Animator.StringToHash(animationBlendName);
        if (speed <= 0) speed = defaultSpeed;

        gravity = (gravity > 0) ? -gravity : gravity;
    }

    private void Update()
    {
        HandleMovement();
        HandleAnimations();
    }

    void HandleMovement()
    {
        HandleGravity();

        velocity.x = InputsManager.Instance.movementInput.x;
        velocity.z = InputsManager.Instance.movementInput.y;

        moveDirection = transform.right * velocity.x + transform.forward * velocity.z;

        finalMovement = new Vector3(moveDirection.x, velocity.y, moveDirection.z);

        characterController.Move(finalMovement * speed * Time.deltaTime);
    }

    void HandleAnimations()
    {
        float moveDirMagnitude = new Vector3(moveDirection.x, 0f, moveDirection.z).magnitude;

        if (moveDirMagnitude > 0.1f)
        {
            characterAnimator.SetFloat(animationBlendHash, 1f, animationSmoothTime, Time.deltaTime);
        }
        else
        {
            characterAnimator.SetFloat(animationBlendHash, 0f, animationSmoothTime, Time.deltaTime);
        }
    }

    void HandleGravity()
    {
        if (characterController.isGrounded)
            velocity.y = defaultGravity;
        else
            velocity.y += gravity * Time.deltaTime;
    }
}