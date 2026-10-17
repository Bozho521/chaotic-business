using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MiniGamePlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 15f;
    public Transform playerCamera; 

    private CharacterController controller;
    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!MainPlayerController.IsPlayingMiniGame) return;

        switch (MainPlayerController.ActiveGameType)
        {
            case MiniGameType.MazeGame:
                HandleMazeGame();
                break;
            case MiniGameType.ValveMinigame:
                HandleValveMinigame();
                break;
            case MiniGameType.TimerGame:
                HandleTimerGame();
                break;
            case MiniGameType.FixingGame:
                HandleFixingGame();
                break;
        }
    }

    private void HandleMazeGame()
    {
        HandleLook();
        HandleMovement();
    }

    private void HandleValveMinigame()
    {
        
    }

    private void HandleTimerGame()
    {
        
    }

    private void HandleFixingGame()
    {
        
    }

    private void HandleLook()
    {
        if (Mouse.current == null || playerCamera == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseDelta.x);
    }

    private void HandleMovement()
    {
        if (Keyboard.current == null || Mouse.current == null) return;

        float x = (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f);
        float z = (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f);

        if (Mouse.current.leftButton.isPressed) z += 1f;
        if (Mouse.current.rightButton.isPressed) z -= 1f;

        z = Mathf.Clamp(z, -1f, 1f);

        Vector3 move = transform.right * x + transform.forward * z;
        if (move.magnitude > 1f) move.Normalize();

        controller.Move(move * moveSpeed * Time.deltaTime + Physics.gravity * Time.deltaTime);
    }
}