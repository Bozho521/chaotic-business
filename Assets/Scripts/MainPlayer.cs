using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MainPlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 15f;
    public Transform mainCamera;
    public LayerMask monitorLayer;
    
    public static bool IsPlayingMiniGame = false;
    public static MiniGameType ActiveGameType = MiniGameType.None;

    private CharacterController controller;
    private float xRotation = 0f;
    
    private Transform activeMonitorZoomPos;
    private Vector3 originalCameraLocalPos;
    private Quaternion originalCameraLocalRot;
    private bool isTransitioning = false;
    private float transitionProgress = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        originalCameraLocalPos = mainCamera.localPosition;
        originalCameraLocalRot = mainCamera.localRotation;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (isTransitioning)
        {
            HandleCameraTransition();
            return;
        }

        if (IsPlayingMiniGame)
        {
            bool pressedEscape = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            bool pressedRMB = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;

            if (pressedEscape || pressedRMB)
            {
                Debug.Log($"<color=yellow>--- EXITING MINIGAME: {ActiveGameType} ---</color>");
                
                IsPlayingMiniGame = false;
                ActiveGameType = MiniGameType.None;
                StartTransition(null);
            }
            return; 
        }

        HandleLook();
        HandleMovement();
        HandleInteraction();
    }

    private void HandleLook()
    {
        if (Mouse.current == null) return;
        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        mainCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseDelta.x);
    }

    private void HandleMovement()
    {
        if (Keyboard.current == null) return;
        Vector3 move = transform.right * ReadAxis(Keyboard.current.aKey, Keyboard.current.dKey) + 
                       transform.forward * ReadAxis(Keyboard.current.sKey, Keyboard.current.wKey);
        
        if (move.magnitude > 1f) move.Normalize();
        controller.Move(move * moveSpeed * Time.deltaTime + Physics.gravity * Time.deltaTime);
    }

    private void HandleInteraction()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = new Ray(mainCamera.position, mainCamera.forward);
            
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, monitorLayer))
            {
                MonitorTarget target = hit.collider.GetComponent<MonitorTarget>();
                if (target != null) 
                {
                    ActiveGameType = target.gameType;
                    Debug.Log($"<color=cyan>Transitioning to: {ActiveGameType}</color>");
                    StartTransition(target.zoomPosition);
                }
            }
        }
    }

    private void StartTransition(Transform targetPos)
    {
        isTransitioning = true;
        transitionProgress = 0f;
        activeMonitorZoomPos = targetPos;
    }

    private void HandleCameraTransition()
    {
        transitionProgress += Time.deltaTime * 3f;
        
        Vector3 targetPos = activeMonitorZoomPos != null ? activeMonitorZoomPos.position : transform.TransformPoint(originalCameraLocalPos);
        Quaternion targetRot = activeMonitorZoomPos != null ? activeMonitorZoomPos.rotation : transform.rotation * originalCameraLocalRot;

        mainCamera.position = Vector3.Lerp(mainCamera.position, targetPos, transitionProgress);
        mainCamera.rotation = Quaternion.Lerp(mainCamera.rotation, targetRot, transitionProgress);

        if (transitionProgress >= 1f)
        {
            isTransitioning = false;
            
            if (activeMonitorZoomPos != null)
            {
                IsPlayingMiniGame = true;
                Debug.Log($"<color=green>--- MINIGAME STARTED: {ActiveGameType} ---</color>");
            }
        }
    }

    private float ReadAxis(UnityEngine.InputSystem.Controls.KeyControl neg, UnityEngine.InputSystem.Controls.KeyControl pos)
    {
        return (pos.isPressed ? 1f : 0f) - (neg.isPressed ? 1f : 0f);
    }
}