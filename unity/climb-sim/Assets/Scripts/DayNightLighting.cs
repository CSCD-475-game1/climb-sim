using UnityEngine;
using UnityEngine.Rendering;

public class DayNightLighting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light sun;

    [Header("Time")]
    [SerializeField] private float dayDurationInSeconds = 60f;
    [Range(0f, 1f)]
    [SerializeField] private float timeOfDay;

    [Header("Sun Rotation")]
    [SerializeField] private Vector3 sunRotationOffset = Vector3.zero;

    [Header("Sun Intensity")]
    [SerializeField] private float maxSunIntensity = 1.2f;
    [SerializeField] private AnimationCurve sunIntensityCurve = new AnimationCurve(
        new Keyframe(0.0f, 0.0f),
        new Keyframe(0.08f, 1.0f),
        new Keyframe(0.20f, 1.2f),
        new Keyframe(0.50f, 0.85f),
        new Keyframe(1.0f, 0.6f)
    );

    [Header("Sun Color")]
    [SerializeField] private Gradient sunColorGradient;

    [Header("Ambient Intensity")]
    [SerializeField] private float dayAmbientIntensity = 1.0f;
    [SerializeField] private float nightAmbientIntensity = 0.02f;

    [Header("Ambient Colors")]
    [SerializeField] private Gradient ambientSkyGradient;
    [SerializeField] private Gradient ambientEquatorGradient;
    [SerializeField] private Gradient ambientGroundGradient;

    [Header("Fog")]
    [SerializeField] private Gradient fogColorGradient;
    [SerializeField] private float dayFogDensity = 0.002f;
    [SerializeField] private float nightFogDensity = 0.008f;

    [Header("Reflections")]
    [SerializeField] private float dayReflectionIntensity = 1.0f;
    [SerializeField] private float nightReflectionIntensity = 0.1f;

    void Start() {
        UpdateSunRotation();
        UpdateLighting();
    }

    private void Update()
    {
        AdvanceTime();
        UpdateSunRotation();
        UpdateLighting();
    }

    private void AdvanceTime()
    {
        Debug.Log($"Time of Day: {timeOfDay:F2}");
        if (dayDurationInSeconds <= 0f) return;

        timeOfDay += Time.deltaTime / dayDurationInSeconds;
        if (timeOfDay > 1f)
            timeOfDay -= 1f;
    }

    private void UpdateSunRotation()
    {
        if (sun == null) return;

        float sunAngle = -(timeOfDay * 360f - 180f);
        sun.transform.rotation = Quaternion.Euler(sunAngle, sunRotationOffset.y - 180f, sunRotationOffset.z);
    }

    private void UpdateLighting()
    {
        if (sun == null) return;

        // -1 to 1 roughly, where positive means above horizon
        float sunHeight = Vector3.Dot(-sun.transform.forward, Vector3.up);

        // 0..1 control value for color transitions
        // this starts warming before the sun reaches the horizon
        float height01 = Mathf.Clamp01((sunHeight + 0.2f) / 1.2f);
        height01 = Mathf.SmoothStep(0f, 1f, height01);

        // Separate daylight factor if you want night to get dark fast
        float daylight = Mathf.Clamp01((sunHeight + 0.15f) / 1.15f);
        daylight = Mathf.SmoothStep(0f, 1f, daylight);

        // Sun intensity from curve
        float intensityMultiplier = Mathf.Max(0f, sunIntensityCurve.Evaluate(height01));
        sun.intensity = maxSunIntensity * intensityMultiplier;

        // Sun color from gradient
        sun.color = sunColorGradient.Evaluate(height01);

        // Ambient
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientIntensity = Mathf.Lerp(nightAmbientIntensity, dayAmbientIntensity, daylight);
        RenderSettings.ambientSkyColor = ambientSkyGradient.Evaluate(height01);
        RenderSettings.ambientEquatorColor = ambientEquatorGradient.Evaluate(height01);
        RenderSettings.ambientGroundColor = ambientGroundGradient.Evaluate(height01);

        // Fog
        RenderSettings.fogColor = fogColorGradient.Evaluate(height01);
        RenderSettings.fogDensity = Mathf.Lerp(nightFogDensity, dayFogDensity, daylight);

        // Reflections
        RenderSettings.reflectionIntensity = Mathf.Lerp(nightReflectionIntensity, dayReflectionIntensity, daylight);
    }
}
