using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Transform originalPosition;
    public Transform monitorZoomPosition;
    public float transitionSpeed = 3f;
    public float swayAmount = 0.5f;
    public LayerMask monitorLayer;

    private enum CameraState { Environment, Screen, Transitioning }
    private CameraState currentState = CameraState.Environment;
    
    private Camera mainCamera;
    private Vector3 startPos;
    private Quaternion startRot;
    private Vector3 targetPos;
    private Quaternion targetRot;
    private float transitionProgress = 0f;

    void Start()
    {
        mainCamera = Camera.main;
        startPos = originalPosition.position;
        startRot = originalPosition.rotation;
        transform.SetPositionAndRotation(startPos, startRot);
    }

    void Update()
    {
        switch (currentState)
        {
            case CameraState.Transitioning:
                HandleTransition();
                break;
            case CameraState.Environment:
                HandleEnvironmentState();
                break;
            case CameraState.Screen:
                HandleScreenState();
                break;
        }
    }

    private void HandleTransition()
    {
        transitionProgress += Time.deltaTime * transitionSpeed;
        transform.SetPositionAndRotation(
            Vector3.Lerp(startPos, targetPos, transitionProgress),
            Quaternion.Lerp(startRot, targetRot, transitionProgress)
        );

        if (transitionProgress >= 1f)
        {
            currentState = (targetPos == monitorZoomPosition.position) ? CameraState.Screen : CameraState.Environment;
        }
    }

    private void HandleEnvironmentState()
    {
        if (Mouse.current == null) return;

        HandleEnvironmentSway();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);
            
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, monitorLayer))
            {
                StartTransition(monitorZoomPosition);
            }
        }
    }

    private void HandleScreenState()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            StartTransition(originalPosition);
        }
    }

    private void HandleEnvironmentSway()
    {
        float mouseX = (Mouse.current.position.ReadValue().x / Screen.width) - 0.5f;
        Vector3 targetSway = originalPosition.position + new Vector3(mouseX * swayAmount, 0, 0);
        transform.position = Vector3.Lerp(transform.position, targetSway, Time.deltaTime * 2f);
    }

    private void StartTransition(Transform target)
    {
        currentState = CameraState.Transitioning;
        startPos = transform.position;
        startRot = transform.rotation;
        targetPos = target.position;
        targetRot = target.rotation;
        transitionProgress = 0f;
    }
}