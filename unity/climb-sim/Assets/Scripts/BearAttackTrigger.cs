using UnityEngine;

public class BearAttackTrigger : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private bool isAttacking = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Bear attack trigger entered by: " + other.gameObject.name);
        animator.Play("Attack1");
        
        if (isAttacking) return;

        if (other.CompareTag("Player"))
        {
            isAttacking = true;
            animator.SetTrigger("Attack");
        }
    }
}
