using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    [Header("Forward Movement")]
    public float walkSpeed = 2f;

    [Header("Bobbing")]
    public float bobSpeed = 3f;
    public float bobAmount = 0.2f;

    [Header("Swaying")]
    public float swayAmount = 0.15f;

    private Vector3 basePosition;
    private float timer = 0f;

    void Start()
    {
        basePosition = transform.position;
    }

    void Update()
    {
        basePosition += transform.forward * walkSpeed * Time.deltaTime;

        timer += Time.deltaTime * bobSpeed;

        float newY = Mathf.Sin(timer) * bobAmount;

        float newX = Mathf.Cos(timer / 2f) * swayAmount;

        transform.position = basePosition + (transform.up * newY) + (transform.right * newX);
    }
}