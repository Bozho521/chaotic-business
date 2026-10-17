using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    [Header("Forward Movement")]
    public float walkSpeed = 2f;

    [Header("Bobbing")]
    public float bobSpeed = 3f;
    public float bobAmount = 0.2f;

    private float defaultPosY;
    private float timer = 0f;

    void Start()
    {
        defaultPosY = transform.position.y;
    }

    void Update()
    {
        transform.position += transform.forward * walkSpeed * Time.deltaTime;

        timer += Time.deltaTime * bobSpeed;
        float newY = defaultPosY + Mathf.Sin(timer) * bobAmount;

        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}