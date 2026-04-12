using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OffRoadWheelCarController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider rearLeft;
    public WheelCollider rearRight;

    [Header("Wheel Meshes")]
    public Transform frontLeftMesh;
    public Transform frontRightMesh;
    public Transform rearLeftMesh;
    public Transform rearRightMesh;

    [Header("Wheel Mesh Rotation Offsets")]
    public Vector3 frontLeftMeshOffset = Vector3.zero;
    public Vector3 frontRightMeshOffset = new Vector3(0f, 180f, 0f);
    public Vector3 rearLeftMeshOffset = Vector3.zero;
    public Vector3 rearRightMeshOffset = new Vector3(0f, 180f, 0f);

    [Header("Driving")]
    public float maxMotorTorque = 2200f;
    public float maxBrakeTorque = 3500f;
    public float maxSteerAngle = 30f;
    public float reverseTorque = 1200f;
    public float maxSpeed = 22f;
    public bool canDrive = true;

    [Header("Off-road Feel")]
    public float handbrakeSidewaysStiffness = 0.5f;
    public float normalSidewaysStiffness = 2.1f;
    public float normalForwardStiffness = 1.2f;

    private Rigidbody rb;
    private float moveInput;
    private float steerInput;
    private bool braking;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 1400f;
        rb.linearDamping = 0.15f;
        rb.angularDamping = 0.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.centerOfMass = new Vector3(0f, -0.4f, 0f);

        ConfigureWheel(frontLeft);
        ConfigureWheel(frontRight);
        ConfigureWheel(rearLeft);
        ConfigureWheel(rearRight);
    }

    void Update()
    {
        if (!canDrive)
        {
            moveInput = 0f;
            steerInput = 0f;
            braking = true;
            return;
        }

        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
        braking = Input.GetKey(KeyCode.Space);
    }

    void FixedUpdate()
    {
        ApplyMotor();
        ApplySteering();
        ApplyBrakes();
        LimitTopSpeed();
        UpdateWheelVisuals();
    }

    void ConfigureWheel(WheelCollider wheel)
    {
        wheel.mass = 35f;
        wheel.suspensionDistance = 0.22f;
        wheel.forceAppPointDistance = 0.1f;

        JointSpring spring = wheel.suspensionSpring;
        spring.spring = 28000f;
        spring.damper = 4500f;
        spring.targetPosition = 0.5f;
        wheel.suspensionSpring = spring;

        WheelFrictionCurve forward = wheel.forwardFriction;
        forward.extremumSlip = 0.4f;
        forward.extremumValue = 1f;
        forward.asymptoteSlip = 0.8f;
        forward.asymptoteValue = 0.75f;
        forward.stiffness = normalForwardStiffness;
        wheel.forwardFriction = forward;

        WheelFrictionCurve sideways = wheel.sidewaysFriction;
        sideways.extremumSlip = 0.2f;
        sideways.extremumValue = 1f;
        sideways.asymptoteSlip = 0.35f;
        sideways.asymptoteValue = 0.85f;
        sideways.stiffness = normalSidewaysStiffness;
        wheel.sidewaysFriction = sideways;
    }

    void ApplyMotor()
    {
        float motor = 0f;

        if (moveInput > 0f)
            motor = moveInput * maxMotorTorque;
        else if (moveInput < 0f)
            motor = moveInput * reverseTorque;

        rearLeft.motorTorque = motor;
        rearRight.motorTorque = motor;
        frontLeft.motorTorque = motor * 0.35f;
        frontRight.motorTorque = motor * 0.35f;
    }

    void ApplySteering()
    {
        float speed = rb.linearVelocity.magnitude;
        float speedFactor = Mathf.InverseLerp(0f, maxSpeed, speed);
        float steerAngle = Mathf.Lerp(maxSteerAngle, maxSteerAngle * 0.5f, speedFactor) * steerInput;

        frontLeft.steerAngle = steerAngle;
        frontRight.steerAngle = steerAngle;
    }

    void ApplyBrakes()
    {
        float brake = braking ? maxBrakeTorque : 0f;

        frontLeft.brakeTorque = brake;
        frontRight.brakeTorque = brake;
        rearLeft.brakeTorque = brake;
        rearRight.brakeTorque = brake;

        float sideways = braking ? handbrakeSidewaysStiffness : normalSidewaysStiffness;
        SetSidewaysStiffness(rearLeft, sideways);
        SetSidewaysStiffness(rearRight, sideways);
    }

    void LimitTopSpeed()
    {
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVelocity.magnitude > maxSpeed)
        {
            Vector3 limited = flatVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(limited.x, rb.linearVelocity.y, limited.z);
        }
    }

    void SetSidewaysStiffness(WheelCollider wheel, float stiffness)
    {
        WheelFrictionCurve sideways = wheel.sidewaysFriction;
        sideways.stiffness = stiffness;
        wheel.sidewaysFriction = sideways;
    }

    void UpdateWheelVisuals()
    {
        UpdateWheelPose(frontLeft, frontLeftMesh, frontLeftMeshOffset);
        UpdateWheelPose(frontRight, frontRightMesh, frontRightMeshOffset);
        UpdateWheelPose(rearLeft, rearLeftMesh, rearLeftMeshOffset);
        UpdateWheelPose(rearRight, rearRightMesh, rearRightMeshOffset);
    }

    void UpdateWheelPose(WheelCollider collider, Transform wheelMesh, Vector3 rotationOffset)
    {
        collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
        wheelMesh.position = pos;
        wheelMesh.rotation = rot * Quaternion.Euler(rotationOffset);
    }
}
