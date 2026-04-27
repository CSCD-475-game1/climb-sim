using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    private UIDocument uiDoc;
    private VisualElement healthBar;
    private ThirstController thirstController;

    [Header("Settings")]
    public float maxHealth;
    public float health;
    public float recoveryRate;

    [Header("Respawn")]
    public Transform respawnPoint;

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

        thirstController = FindObjectOfType<ThirstController>();
    }

    void Update()
    {
        if (health < maxHealth && (thirstController == null || thirstController.currentThirst > 0))
        {
            health += recoveryRate * Time.deltaTime;
            health = Mathf.Clamp(health, 0f, maxHealth);
            UpdateHealthBar();
        }
    }

    public void RestoreHealth(float amount)
    {
        health = Mathf.Clamp(health + amount, 0f, maxHealth);
        UpdateHealthBar();
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        health = Mathf.Clamp(health, 0f, maxHealth);

        Debug.Log("Player health: " + health);

        //if (DamageScreenFX.Instance != null)
            //DamageScreenFX.Instance.PlayDamageFlash();

        if (amount > 15 && CameraShake.Instance != null)
            CameraShake.Instance.Shake();

        UpdateHealthBar();

        if (health <= 0f)
        {
            Debug.Log("Player died");

            if (respawnPoint == null)
            {
                Debug.LogError("Respawn point is not assigned.");
                return;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("No GameObject with tag 'Player' found.");
                return;
            }

            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller == null)
            {
                Debug.LogError("No CharacterController found on Player root.");
                return;
            }

            controller.enabled = false;

            player.transform.position = respawnPoint.position;
            player.transform.rotation = respawnPoint.rotation;

            health = maxHealth;
            UpdateHealthBar();

            controller.enabled = true;

            PlayerPrefs.SetString("GameResult", "Game Over! You ded.");
            SceneManager.LoadScene("StartScreen", LoadSceneMode.Single);
        }
    }


    void UpdateHealthBar()
    {
        if (healthBar == null) return;

        float percentage = Mathf.Clamp((health / maxHealth) * 100f, 0f, 100f);
        healthBar.style.width = Length.Percent(percentage);
    }
}
