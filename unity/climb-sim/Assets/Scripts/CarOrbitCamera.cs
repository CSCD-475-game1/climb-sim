using UnityEngine;

public class CarOrbitCamera : MonoBehaviour
{
    public Transform target;          // Car transform
    public Vector3 offset = new Vector3(0f, 2.5f, -6f);

    public float mouseSensitivity = 3f;
    public float minPitch = -10f;
    public float maxPitch = 60f;

    public float followSmooth = 10f;

    private float yaw = 0f;
    private float pitch = 15f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 desiredPosition = target.position + rotation * offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSmooth * Time.deltaTime
        );

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
