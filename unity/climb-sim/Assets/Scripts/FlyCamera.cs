using UnityEngine;

public class FlyCamera : MonoBehaviour
{
    public float speed = 20f;
    public float mouseSensitivity = 2f;
    public float fastMultiplier = 3f;

    float rotationX = 0f;
    float rotationY = 0f;

    void Update()
    {
        // Mouse look (right click to activate)
        if (Input.GetMouseButton(1))
        {
            rotationX += Input.GetAxis("Mouse X") * mouseSensitivity * 100f * Time.deltaTime;
            rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity * 100f * Time.deltaTime;
            rotationY = Mathf.Clamp(rotationY, -90f, 90f);

            transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0f);
        }

        // Movement
        float moveSpeed = speed;
        if (Input.GetKey(KeyCode.LeftShift))
            moveSpeed *= fastMultiplier;

        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S

        float up = 0f;
        if (Input.GetKey(KeyCode.E)) up = 1f;
        if (Input.GetKey(KeyCode.Q)) up = -1f;

        Vector3 move = transform.right * h + transform.forward * v + Vector3.up * up;
        transform.position += move * moveSpeed * Time.deltaTime;
    }
}
