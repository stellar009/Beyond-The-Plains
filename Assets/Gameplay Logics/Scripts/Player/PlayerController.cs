using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private Animator characterAnimator;
    private float defaultSpeed = 2f;
    private float defaultGravity = -2f;

    public float speed = 2f;

    [Tooltip("Gravity can be entered positive too")]
    public float gravity = 9.81f;

    [Header("Animation Settings")]
    public string animationBlendName = "Speed";
    public float animationSmoothTime = 0.2f;

    private Vector3 velocity;
    private int animationBlendValue;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (!characterController) Debug.Log("No character Found");

        characterAnimator = GetComponentInChildren<Animator>();
        if (!characterAnimator) Debug.Log("No Animators");
    }

    private void Start()
    {
        animationBlendValue = Animator.StringToHash(animationBlendName);
        if (speed <= 0) speed = defaultSpeed;

        gravity = (gravity > 0) ? -gravity : gravity;
    }

    private void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        velocity.x = InputsManager.Instance.movementInput.x * speed * Time.deltaTime;
        velocity.z = InputsManager.Instance.movementInput.y * speed * Time.deltaTime;

        if (!characterController.isGrounded)
            velocity.y += gravity * Time.deltaTime;
        else
            velocity.y += defaultGravity;

        velocity = velocity.normalized;

        characterController.Move(velocity);
    }
}