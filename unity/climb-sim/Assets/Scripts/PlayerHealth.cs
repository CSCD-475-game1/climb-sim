using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHealth : MonoBehaviour
{
    private UIDocument uiDoc;
    private VisualElement healthBar;

    [Header("Settings")]
    public float maxHealth = 100f;
    public float health = 100f;

    void Awake()
    {
        uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("No UIDocument found on PlayerHealth object.");
            return;
        }
    }

    void Start()
    {
        if (uiDoc != null)
        {
            var root = uiDoc.rootVisualElement;
            healthBar = root.Q<VisualElement>("HealthBar");

            if (healthBar == null)
            {
                Debug.LogError("Could not find VisualElement named 'HealthBar'.");
            }
        }

        health = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        health = Mathf.Clamp(health, 0f, maxHealth);

        Debug.Log("Player health: " + health);

        UpdateHealthBar();

        if (health <= 0f)
        {
            Debug.Log("Player died");
        }
    }


    void UpdateHealthBar()
    {
        if (healthBar == null) return;

        float percentage = Mathf.Clamp((health / maxHealth) * 100f, 0f, 100f);
        healthBar.style.width = Length.Percent(percentage);
    }
}