using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class NpcHikerController : MonoBehaviour
{

    [SerializeField] private Transform graphics;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float crawlSpeed = 0.8f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Slope Detection")]
    [SerializeField] private float groundCheckDistance = 2.0f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float enterCrawlAngle = 40f;
    [SerializeField] private float exitCrawlAngle = 32f;

    [Header("Target")]
    [SerializeField] private Transform moveTarget;

    private Rigidbody rb;
    private Animator animator;

    private HikerMoveState currentState = HikerMoveState.Walk;
    private RaycastHit groundHit;
    private bool isGrounded;
    private float currentSlopeAngle;

    public float detectRange = 8f;
    public float stopDistance = 1.5f;
    public float moveSpeed = 2f;
    public float turnSpeed = 8f;
    public float heightOffset = 0.05f;

    private bool crawlingDownhill = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        //rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
    private void Start()
    {
        CheckGround();
        //AlignGraphicsToSlope();

        //if (!isGrounded) return;

        //Vector3 uphill = Vector3.ProjectOnPlane(Vector3.up, groundHit.normal).normalized;

        //if (uphill.sqrMagnitude > 0.001f)
        //{
            //transform.rotation = Quaternion.LookRotation(uphill, groundHit.normal);
        //}

        //if (currentSlopeAngle >= enterCrawlAngle)
        //{
            //currentState = HikerMoveState.Crawl;
            //animator.SetBool("IsCrawling", true);
            //animator.SetFloat("Speed", 0f);
        //}
    }

    private void Update()
    {
        //rb.useGravity = currentState != HikerMoveState.Crawl;
        //CheckGround();
        //AlignGraphicsToSlope();
        //UpdateState();
        //UpdateAnimator();
        
        if (moveTarget == null) return;

        Vector3 currentPos = transform.position;
        Vector3 playerPos = moveTarget.position;

        Vector3 toPlayer = playerPos - currentPos;
        toPlayer.y = 0f;

        float distance = toPlayer.magnitude;

        // Too far away: do nothing
        if (distance > detectRange)
        {
            animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
            //SnapToTerrain();
            SnapToGround();
            return;
        }

        // Within detection range but not close enough yet: follow
        if (distance > stopDistance)
        {
            Vector3 moveDir = toPlayer.normalized;

            currentPos += moveDir * moveSpeed * Time.deltaTime;
            transform.position = currentPos;

            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );

            animator.SetFloat("Speed", 1f, 0.1f, Time.deltaTime);
        }
        else
        {
            animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
        }

        animator.SetFloat("Direction", 0f);

        //SnapToTerrain();
        SnapToGround();
    }

    //private void LateUpdate()
    //{
        //AlignGraphicsToSlope();
    //}

    private void FixedUpdate()
    {
        //if (!isGrounded) return

        if (currentState == HikerMoveState.Crawl)
        {
            CrawlTowardPlayerFacingInward();
        }
        else
        {
            //MoveTowardTarget();
        }
    }

    private void AlignGraphicsToSlope()
    {
        if (!isGrounded || graphics == null) return;

        if (currentState == HikerMoveState.Walk) return;

        Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundHit.normal).normalized;
        Vector3 uphill = -downhill;

        Quaternion slopeRotation = Quaternion.LookRotation(uphill, groundHit.normal);
        graphics.localRotation = Quaternion.Inverse(transform.rotation) * slopeRotation;
    }

    private void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.2f;

        isGrounded = Physics.SphereCast(
            origin,
            0.3f,                      // radius
            Vector3.down,
            out groundHit,
            groundCheckDistance,
            groundMask
        );
        if (isGrounded)
        {
            currentSlopeAngle = Vector3.Angle(groundHit.normal, Vector3.up);
        }
        else
        {
            currentSlopeAngle = 0f;
        }
    }

    private void UpdateState()
    {
        if (!isGrounded) return;

        switch (currentState)
        {
            case HikerMoveState.Walk:
                if (currentSlopeAngle >= enterCrawlAngle)
                    currentState = HikerMoveState.Crawl;
                break;

            case HikerMoveState.Crawl:
                if (currentSlopeAngle <= exitCrawlAngle)
                    currentState = HikerMoveState.Walk;
                break;
        }
        Debug.Log($"Slope Angle: {currentSlopeAngle:F1} | State: {currentState}");
    }

    private void CrawlDownSlope()
    {
        if (!isGrounded) return;

        Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundHit.normal).normalized;
        if (downhill.sqrMagnitude < 0.001f) return;

        Vector3 desiredVelocity = downhill * crawlSpeed;
        rb.linearVelocity = new Vector3(desiredVelocity.x, rb.linearVelocity.y, desiredVelocity.z);
    }
    private void CrawlTowardPlayerFacingInward()
    {
        Vector3 moveDir = GetDownhillTowardTargetDirection();

        if (moveDir.sqrMagnitude < 0.001f)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 desiredVelocity = moveDir * crawlSpeed;
        rb.linearVelocity = new Vector3(desiredVelocity.x, 0f, desiredVelocity.z);
    }

    private Vector3 GetDownhillTowardTargetDirection()
    {
        if (!isGrounded || moveTarget == null)
            return Vector3.zero;

        // Pure downhill along the slope
        Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundHit.normal).normalized;
        if (downhill.sqrMagnitude < 0.001f)
            return Vector3.zero;

        // Direction toward player, constrained to the slope
        Vector3 toTarget = moveTarget.position - transform.position;
        Vector3 towardTargetOnSlope = Vector3.ProjectOnPlane(toTarget, groundHit.normal).normalized;

        if (towardTargetOnSlope.sqrMagnitude < 0.001f)
            return downhill;

        // If target direction is uphill, don't use it directly.
        // Blend toward downhill so NPC still retreats down the slope.
        float targetVsDownhill = Vector3.Dot(towardTargetOnSlope, downhill);

        if (targetVsDownhill <= 0f)
        {
            return downhill;
        }

        // Blend mostly downhill, partly toward target
        Vector3 blended = (downhill * 0.7f + towardTargetOnSlope * 0.3f).normalized;
        return blended;
    }

    private void MoveTowardTarget()
    {
        Vector3 toTarget = moveTarget.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.05f)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        Vector3 desiredDir = toTarget.normalized;

        // Project movement onto slope so the NPC follows the terrain
        Vector3 slopeMoveDir = Vector3.ProjectOnPlane(desiredDir, groundHit.normal).normalized;

        float speed = currentState == HikerMoveState.Crawl ? crawlSpeed : walkSpeed;

        Vector3 desiredVelocity = slopeMoveDir * speed;
        rb.linearVelocity = new Vector3(desiredVelocity.x, rb.linearVelocity.y, desiredVelocity.z);

        if (slopeMoveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(slopeMoveDir, Vector3.up);
            Quaternion newRot = Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRot);
        }
    }

    private void UpdateAnimator()
    {
        float speedPercent = 0f;

        Vector3 horizontalVel = rb.linearVelocity;
        horizontalVel.y = 0f;
        speedPercent = horizontalVel.magnitude;

        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsCrawling", currentState == HikerMoveState.Crawl);
        animator.SetFloat("Speed", speedPercent);
        animator.SetFloat("SlopeAngle", currentSlopeAngle);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position + Vector3.up * 0.2f;
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
    }
    
    void SnapToGround()
    {
        Vector3 rayStart = transform.position + Vector3.up * 2f;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 10f, groundMask))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y + heightOffset;
            transform.position = pos;
        }
    }
}

public enum HikerMoveState
{
    Walk,
    Crawl
}
