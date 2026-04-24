using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BearWander : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0.6f;
    [SerializeField] private float turnSpeed = 2f;
    [SerializeField] private float directionChangeInterval = 4f;
    [SerializeField] private float walkRadius = 6f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float rayStartHeight = 3f;
    [SerializeField] private float rayDistance = 10f;
    [SerializeField] private float groundOffset = 0.05f;

    private Vector3 targetPoint;
    private float timer;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        PickNewTarget();
    }

    private void FixedUpdate()
    {

        timer -= Time.fixedDeltaTime;

        if (timer <= 0f || Vector3.Distance(rb.position, targetPoint) < 0.5f)
        {
            PickNewTarget();
        }

        Vector3 flatDir = targetPoint - rb.position;
        flatDir.y = 0f;

        if (flatDir.sqrMagnitude <= 0.01f)
            return;

        Vector3 moveDir = flatDir.normalized;

        Quaternion targetRot = Quaternion.LookRotation(moveDir);
        Quaternion smoothRot = Quaternion.Slerp(
            rb.rotation,
            targetRot,
            turnSpeed * Time.fixedDeltaTime
        );
        rb.MoveRotation(smoothRot);

        Vector3 candidatePos = rb.position + transform.forward * moveSpeed * Time.fixedDeltaTime;

        if (TryGetGround(candidatePos, out RaycastHit hit))
        {
            candidatePos.y = hit.point.y + groundOffset;
        }

        rb.MovePosition(candidatePos);
    }

    public void SetActive(bool active)
    {
        enabled = active;
    }

    private void PickNewTarget()
    {
        timer = directionChangeInterval + Random.Range(-1f, 1f);

        Vector2 offset2D = Random.insideUnitCircle * walkRadius;
        targetPoint = rb != null
            ? rb.position + new Vector3(offset2D.x, 0f, offset2D.y)
            : transform.position + new Vector3(offset2D.x, 0f, offset2D.y);
    }

    private bool TryGetGround(Vector3 worldPos, out RaycastHit hit)
    {
        Vector3 rayStart = worldPos + Vector3.up * rayStartHeight;
        return Physics.Raycast(rayStart, Vector3.down, out hit, rayDistance, groundMask);
    }
}
