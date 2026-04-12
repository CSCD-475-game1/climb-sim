using UnityEngine;

public class AnimationControl : MonoBehaviour
{
    public Transform player;

    public float detectRange = 8f;
    public float stopDistance = 1.5f;
    public float moveSpeed = 2f;
    public float turnSpeed = 8f;
    public float heightOffset = 0.05f;
    public LayerMask groundMask;

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null) return;

        Vector3 currentPos = transform.position;
        Vector3 playerPos = player.position;

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

    void SnapToTerrain()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null) return;

        Vector3 pos = transform.position;
        float terrainY = terrain.SampleHeight(pos) + terrain.transform.position.y;
        pos.y = terrainY + heightOffset;
        transform.position = pos;
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
