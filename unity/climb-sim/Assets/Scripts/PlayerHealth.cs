using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float health = 100f;

    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log("Player health: " + health);

        if (health <= 0f)
        {
            Debug.Log("Player died");
        }
    }
}
