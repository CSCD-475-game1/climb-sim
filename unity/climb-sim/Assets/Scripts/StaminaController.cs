using UnityEngine;
using UnityEngine.UIElements;

public class StaminaController : MonoBehaviour
{
    private VisualElement _staminaBarInner;
    
    [Header("Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float consumptionRate = 20f;
    public float regenRate = 15f;

    void Start()
    {
        Debug.Log("1. Script is alive on: " + gameObject.name);
        
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("2. ERROR: No UI Document found on this object");
            return;
        }

        var root = uiDoc.rootVisualElement;
        _staminaBarInner = root.Q<VisualElement>("StaminaBar");

        if (_staminaBarInner == null)
        {
            Debug.LogError("3. ERROR: Could not find a Visual Element named 'StaminaBar'");
        }
        else
        {
            Debug.Log("4. SUCCESS: StaminaBar found and connected");
        }

        currentStamina = maxStamina;
    }

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _staminaBarInner = root.Q<VisualElement>("StaminaBar");
        
        currentStamina = maxStamina;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0)
        {
            currentStamina -= consumptionRate * Time.deltaTime;
            Debug.Log("Draining: " + currentStamina);
        }
        else if (currentStamina < maxStamina)
        {
            currentStamina += regenRate * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        
        UpdateStaminaBar();
    }

    void UpdateStaminaBar()
    {
        if (_staminaBarInner != null)
        {
            float ratio = currentStamina / maxStamina;

            float percentage = Mathf.Clamp(ratio * 100f, 0f, 100f);

            _staminaBarInner.style.width = new StyleLength(Length.Percent(percentage));
        
            _staminaBarInner.style.height = new StyleLength(Length.Percent(100f));
        }
    }
}