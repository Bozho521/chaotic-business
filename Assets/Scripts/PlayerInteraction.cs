using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float interactDistance = 3f;
    public LayerMask interactLayer = Physics.DefaultRaycastLayers;

    [Header("Crosshair UI")]
    public RectTransform crosshairRect;
    public float normalSize = 8f;
    public float hoverSize = 16f;
    public float scaleSpeed = 15f;

    private Camera mainCamera;
    private Interactable currentInteractable;

    void Start()
    {
        mainCamera = Camera.main;
        
        if (crosshairRect != null)
        {
            crosshairRect.sizeDelta = new Vector2(normalSize, normalSize);
            crosshairRect.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (MainPlayerController.IsPlayingMiniGame)
        {
            if (crosshairRect != null && crosshairRect.gameObject.activeSelf)
            {
                crosshairRect.gameObject.SetActive(false);
            }
            return;
        }

        if (crosshairRect != null && !crosshairRect.gameObject.activeSelf)
        {
            crosshairRect.gameObject.SetActive(true);
        }

        CheckForInteractable();

        if (currentInteractable != null)
        {
            SetCrosshairSize(hoverSize);

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                currentInteractable.Interact();
            }
        }
        else
        {
            SetCrosshairSize(normalSize);
        }
    }

    private void CheckForInteractable()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            currentInteractable = hit.collider.GetComponent<Interactable>();
        }
        else
        {
            currentInteractable = null;
        }
    }

    private void SetCrosshairSize(float targetSize)
    {
        if (crosshairRect == null) return;

        float newSize = Mathf.Lerp(crosshairRect.sizeDelta.x, targetSize, Time.deltaTime * scaleSpeed);
        crosshairRect.sizeDelta = new Vector2(newSize, newSize);
    }
}