using UnityEngine;
using UnityEngine.InputSystem;

public class InputsManager : MonoBehaviour
{
    public static InputsManager Instance;

    private GameInteractions gameInteractions;

    public Vector2 movementInput { get; private set; }
    public Vector2 cameraInputs {  get; private set; }

    private bool isPaused;

    private void Awake()
    {
        gameInteractions = new GameInteractions();
        ShowCursors(false);
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        gameInteractions.Player.Enable();

        gameInteractions.Player.Movemnet.performed += OnMovementPerformend;
        gameInteractions.Player.Movemnet.canceled += OnMovementCanceled;

        gameInteractions.Player.Camera.performed += OnCameraInputPerformed;
        gameInteractions.Player.Camera.canceled += OnCameraInputCanceled;

        gameInteractions.Player.PauseGame.performed += PauseGame;
    }

    private void OnDisable()
    {
        gameInteractions.Player.Disable();

        gameInteractions.Player.Movemnet.performed -= OnMovementPerformend;
        gameInteractions.Player.Movemnet.canceled -= OnMovementCanceled;

        gameInteractions.Player.Camera.performed -= OnCameraInputPerformed;
        gameInteractions.Player.Camera.canceled -= OnCameraInputCanceled;

        gameInteractions.Player.PauseGame.performed -= PauseGame;
    }

    void OnMovementPerformend(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();
    }

    void OnMovementCanceled(InputAction.CallbackContext ctx)
    {
        movementInput = Vector2.zero;
    }

    void OnCameraInputPerformed(InputAction.CallbackContext ctx)
    {
        cameraInputs = ctx.ReadValue<Vector2>();
    }

    void OnCameraInputCanceled(InputAction.CallbackContext ctx)
    {
        cameraInputs = Vector2.zero;
    }

    void PauseGame(InputAction.CallbackContext ctx)
    {
        isPaused = !isPaused;
        PauseGame(isPaused);
    }

    public void PauseGame(bool state)
    {
        if(state)
        {
            gameInteractions.Player.Movemnet.Enable();
            gameInteractions.Player.Camera.Enable();
        }
        else
        {
            gameInteractions.Player.Movemnet.Disable();
            gameInteractions.Player.Camera.Disable();
        }
        ShowCursors(!state);
    }

    void ShowCursors(bool status)
    {
        Cursor.visible = status;
        Cursor.lockState = status ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
