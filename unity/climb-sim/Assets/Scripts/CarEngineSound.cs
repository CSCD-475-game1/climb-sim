using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CarEngineSound : MonoBehaviour
{
    [SerializeField] private Rigidbody carRb;
    [SerializeField] private AudioClip idleClip;
    [SerializeField] private AudioClip driveClip;

    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 2.0f;
    [SerializeField] private OffRoadWheelCarController carController; 

    private AudioSource audioSource;
    private bool carCanDrive;

    private void Start()
    {
        carCanDrive = carController.GetCanDrive();
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = idleClip;
        audioSource.loop = true;

        if (carCanDrive)
            audioSource.Play();
    }

    private void Update()
    {
        carCanDrive = carController.GetCanDrive();
        if (!carCanDrive) {
            if (audioSource.isPlaying)
                audioSource.Stop();
            return;
        }
        else if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
        float speed = carRb.linearVelocity.magnitude;

        if (speed > 2f && audioSource.clip != driveClip)
        {
            audioSource.clip = driveClip;
            audioSource.Play();
        }
        else if (speed <= 2f && audioSource.clip != idleClip)
        {
            audioSource.clip = idleClip;
            audioSource.Play();
        }

        audioSource.pitch = Mathf.Lerp(
            minPitch,
            maxPitch,
            speed / 30f
        );
    }
}
