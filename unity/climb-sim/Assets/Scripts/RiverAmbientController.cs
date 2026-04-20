using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class RiverAmbientController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SplineContainer riverSpline;
    [SerializeField] private Transform player;
    [SerializeField] private AudioSource audioSource;

    [Header("Sampling")]
    [SerializeField] private int sampleCount = 100;

    [Header("Fade")]
    [SerializeField] private float fullVolumeDistance = 8f;
    [SerializeField] private float silentDistance = 40f;
    [SerializeField] private float maxVolume = 0.35f;
    [SerializeField] private float fadeSpeed = 2.5f;

    private readonly List<Vector3> samplePoints = new();
    private float currentVolume = 0f;

    private void Start()
    {
        CacheSamplePoints();

        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.spatialBlend = 0f; // 2D ambient, volume controlled by distance
            audioSource.volume = 0f;
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
    }

    private void Update()
    {
        if (riverSpline == null || player == null || audioSource == null || samplePoints.Count == 0)
            return;

        float distance = GetDistanceToRiver(player.position);

        float targetVolume;
        if (distance <= fullVolumeDistance)
        {
            targetVolume = maxVolume;
        }
        else if (distance >= silentDistance)
        {
            targetVolume = 0f;
        }
        else
        {
            float t = Mathf.InverseLerp(silentDistance, fullVolumeDistance, distance);
            targetVolume = t * maxVolume;
        }

        currentVolume = Mathf.Lerp(currentVolume, targetVolume, Time.deltaTime * fadeSpeed);
        audioSource.volume = currentVolume;
    }

    private void CacheSamplePoints()
    {
        samplePoints.Clear();

        if (riverSpline == null || riverSpline.Splines.Count == 0)
            return;

        var spline = riverSpline.Splines[0];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = sampleCount == 1 ? 0f : i / (float)(sampleCount - 1);
            Vector3 localPoint = spline.EvaluatePosition(t);
            Vector3 worldPoint = riverSpline.transform.TransformPoint(localPoint);
            samplePoints.Add(worldPoint);
        }
    }

    private float GetDistanceToRiver(Vector3 worldPos)
    {
        float bestSqr = float.MaxValue;

        foreach (Vector3 p in samplePoints)
        {
            float sqr = (worldPos - p).sqrMagnitude;
            if (sqr < bestSqr)
                bestSqr = sqr;
        }

        return Mathf.Sqrt(bestSqr);
    }
}
