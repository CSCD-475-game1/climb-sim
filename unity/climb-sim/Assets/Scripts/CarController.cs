using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    public float maxSpeed = 18f;
    public float acceleration = 35f;
    public float reverseAcceleration = 18f;
    public float turnSpeed = 55f;
    public float linearDamping = 1.2f;
    public float minTurnSpeed = 1f;
    public float grip = 4f;
    public bool canDrive = false;

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearDamping = linearDamping;
        rb.angularDamping = 3f;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        if (!canDrive)
        {
            moveInput = 0f;
            turnInput = 0f;
            return;
        }

        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
    }
    
    public bool GetCanDrive()
    {
        return canDrive;
    }

    void FixedUpdate()
    {
        if (!canDrive) return;

        ApplyDrive();
        ApplySteering();
        ApplyGrip();
    }

    void ApplyDrive()
    {
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float forwardSpeed = Vector3.Dot(flatVelocity, transform.forward);

        float accel = moveInput >= 0f ? acceleration : reverseAcceleration;

        if (Mathf.Abs(forwardSpeed) < maxSpeed || Mathf.Sign(moveInput) != Mathf.Sign(forwardSpeed))
        {
            Vector3 driveDir = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            rb.AddForce(driveDir * moveInput * accel, ForceMode.Acceleration);
        }
    }

    void ApplySteering()
    {
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float speed = flatVelocity.magnitude;

        if (speed < minTurnSpeed) return;
        if (Mathf.Abs(turnInput) < 0.01f) return;

        // Strong at low speed, gentler at high speed
        float steerFactor = 1f - Mathf.Clamp01(speed / maxSpeed) * 0.5f;

        float reverseFactor = moveInput < 0f ? -1f : 1f;

        float turn = turnInput * turnSpeed * steerFactor * reverseFactor * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
    }    

    void ApplyGrip()
    {
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 right = new Vector3(transform.right.x, 0f, transform.right.z).normalized;

        float forwardAmount = Vector3.Dot(flatVelocity, forward);
        float sidewaysAmount = Vector3.Dot(flatVelocity, right);

        // Less grip than a road car
        sidewaysAmount = Mathf.Lerp(sidewaysAmount, 0f, grip * Time.fixedDeltaTime);

        Vector3 correctedFlatVelocity = forward * forwardAmount + right * sidewaysAmount;
        rb.linearVelocity = new Vector3(correctedFlatVelocity.x, rb.linearVelocity.y, correctedFlatVelocity.z);
    }
}
