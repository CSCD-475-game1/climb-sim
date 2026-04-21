using UnityEngine;
using UnityEngine.UIElements;

public class ThirstController : MonoBehaviour
{
    private UIDocument uiDoc;
    private VisualElement thirstBar;

    [Header("Settings")]
    public float maxThirst = 100f;
    public float currentThirst = 100f;

    //change this to adjust the rate of drain
    public float drainRate = 0.5f;

    void Awake()
    {
        uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("No UIDocument found on ThirstController object.");
            enabled = false;
            return;
        }
    }

    void Start()
    {
        var root = uiDoc.rootVisualElement;
        thirstBar = root.Q<VisualElement>("ThirstBar");

        if (thirstBar == null)
        {
            Debug.LogError("Could not find VisualElement named 'ThirstBar'.");
            enabled = false;
            return;
        }

        currentThirst = maxThirst;
        UpdateThirstBar();
    }

    void Update()
    {
        //Constantly drain over time
        if (currentThirst > 0f)
        {
            currentThirst -= drainRate * Time.deltaTime;
        }

        currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);
        UpdateThirstBar();
    }

    public void RestoreThirst(float amount)
    {
        currentThirst += amount;
        currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);
        UpdateThirstBar();
    }

    void UpdateThirstBar()
    {
        if (thirstBar == null) return;

        float percentage = Mathf.Clamp((currentThirst / maxThirst) * 100f, 0f, 100f);
        thirstBar.style.width = Length.Percent(percentage);
    }
}
