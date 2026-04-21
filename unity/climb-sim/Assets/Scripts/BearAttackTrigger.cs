using UnityEngine;

public class BearAttackTrigger : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player"))
        {
            // set bear status to attacking
            Debug.Log("Bear attack trigger entered by: " + other.gameObject.name);
            animator.SetBool("Attack1", true);
            PlayerHealth ph = other.GetComponentInChildren<PlayerHealth>(); 
            if (ph != null)
            {
                ph.TakeDamage(20); 
            }
        }
    }
}
