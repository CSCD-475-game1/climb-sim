using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BearController : MonoBehaviour
{
    [SerializeField] private float fleeSpeed = 4f;
    [SerializeField] private float fleeDuration = 3f;
    [SerializeField] private float turnSpeed = 6f;

    [Header("Grounding")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float rayStartHeight = 3f;
    [SerializeField] private float rayDistance = 10f;
    [SerializeField] private float groundOffset = 0.05f;

    private Vector3 fleeDirection;
    private float fleeTimer;
    private bool isFleeing;

    private Transform player;
    private Animator animator;
    private Rigidbody rb;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;

        // Freeze only X/Z tilt, not Y turning
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (animator != null)
        {
            animator.SetBool("WalkForward", true);
        }
    }

    void FixedUpdate()
    {
        if (!isFleeing)
            return;

        fleeTimer -= Time.fixedDeltaTime;

        if (fleeDirection.sqrMagnitude < 0.001f)
            return;

        // Ground under current position
        if (!TryGetGround(rb.position, out RaycastHit currentGround))
            return;

        // Move direction along slope
        Vector3 moveDir = Vector3.ProjectOnPlane(fleeDirection, currentGround.normal).normalized;

        // Horizontal next position first
        Vector3 candidatePos = rb.position + moveDir * fleeSpeed * Time.fixedDeltaTime;

        // Ground under next position
        if (TryGetGround(candidatePos, out RaycastHit nextGround))
        {
            candidatePos.y = nextGround.point.y + groundOffset;
        }

        rb.MovePosition(candidatePos);

        // Turn to face movement direction
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, currentGround.normal);
            Quaternion smoothRot = Quaternion.Slerp(rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(smoothRot);
        }

        if (fleeTimer <= 0f)
        {
            isFleeing = false;

            if (animator != null)
            {
                animator.SetBool("Run Forward", false);
            }
        }
    }

    public void Repel()
    {
        if (player == null) return;

        Vector3 awayFromPlayer = transform.position - player.position;
        awayFromPlayer.y = 0f;

        if (awayFromPlayer.sqrMagnitude < 0.001f)
            awayFromPlayer = -transform.forward;

        fleeDirection = awayFromPlayer.normalized;
        fleeTimer = fleeDuration;
        isFleeing = true;

        if (animator != null)
        {
            animator.SetBool("Attack1", false);
            animator.SetBool("Run Forward", true);
        }
    }

    private bool TryGetGround(Vector3 worldPos, out RaycastHit hit)
    {
        Vector3 rayStart = worldPos + Vector3.up * rayStartHeight;
        return Physics.Raycast(rayStart, Vector3.down, out hit, rayDistance, groundMask);
    }
}
