using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public float dayDurationInSeconds = 60f; // Total real-world seconds for one full day

    void Update() {
        float rotationSpeed = 360f / dayDurationInSeconds;
        transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
    }

}