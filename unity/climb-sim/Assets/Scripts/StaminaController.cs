using UnityEngine;
using UnityEngine.UIElements;

public class StaminaController : MonoBehaviour
{
    private UIDocument uiDoc;
    private VisualElement staminaBar;

    [Header("Settings")]
    public float maxStamina = 100f;
    public float currentStamina = 100f;
    public float consumptionRate = 20f;
    public float regenRate = 15f;
    public bool IsExhausted => currentStamina <= 10.0f;

    void Awake()
    {
        uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("No UIDocument found on this object.");
            enabled = false;
            return;
        }
    }

    void Start()
    {
        var root = uiDoc.rootVisualElement;

        staminaBar = root.Q<VisualElement>("StaminaBar");

        if (staminaBar == null)
        {
            Debug.LogError("Could not find VisualElement named 'StaminaBar'.");
            enabled = false;
            return;
        }

        Debug.Log("Found element: " + staminaBar.name);

        currentStamina = maxStamina;
        UpdateStaminaBar();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0f)
            currentStamina -= consumptionRate * Time.deltaTime;
        else if (currentStamina < maxStamina)
            currentStamina += regenRate * Time.deltaTime;

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        UpdateStaminaBar();
    }

    void UpdateStaminaBar()
    {
        if (staminaBar == null) return;

        float percentage = Mathf.Clamp((currentStamina / maxStamina) * 100f, 0f, 100f);
        staminaBar.style.width = Length.Percent(percentage);

        Debug.Log($"Percent: {percentage}, resolved width: {staminaBar.resolvedStyle.width}");
    }
}
