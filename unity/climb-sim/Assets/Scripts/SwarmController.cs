using UnityEngine;

public class SwarmController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float hoverStrength = 0.4f;
    public float hoverSpeed = 3f;

    public float loseTargetDistance = 20f;
    public float forgetAfterSeconds = 5f;

    private Transform target;
    private float forgetTimer;

    void Update()
    {
        if (target != null)
        {
            FollowTarget();

            //if (Vector3.Distance(transform.position, target.position) > loseTargetDistance)
            //{
                //ClearTarget();
            //}

            //forgetTimer -= Time.deltaTime;
            //if (forgetTimer <= 0f)
            //{
                //ClearTarget();
            //}
        }
    }

    void FollowTarget()
    {
        Vector3 desiredPos = target.position;
        desiredPos.y += 1.5f; // hover near upper body/head

        desiredPos.y += Mathf.Sin(Time.time * hoverSpeed) * hoverStrength;

        transform.position = Vector3.MoveTowards(
            transform.position,
            desiredPos,
            moveSpeed * Time.deltaTime
        );
    }

    void ClearTarget()
    {
        target = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Swarm detected player!");
            target = other.transform;
            //forgetTimer = forgetAfterSeconds;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //if (other.transform == target)
        //{
            //forgetTimer = forgetAfterSeconds;
        //}
    }

    private void OnTriggerExit(Collider other)
    {
        //if (other.transform == target)
        //{
            //forgetTimer = 2f; // brief pursuit after leaving trigger
        //}
    }
}
