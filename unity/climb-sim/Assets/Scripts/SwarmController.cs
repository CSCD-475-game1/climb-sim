using UnityEngine;

public class SwarmController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float hoverStrength = 0.4f;
    public float hoverSpeed = 3f;

    public float loseTargetDistance = 20f;
    public float forgetAfterSeconds = 5f;

    private Transform target;
    private float forgetTimer;

    [Header("ChatUI")]
    [SerializeField] private ChatUIManager chatUI;

    [Header("Particles")]
    [SerializeField] private ParticleSystem ps;

    private float baseEmissionRate;
    private float currentRate;

    [Header("Audio")]
    [SerializeField] private MosquitoBuzzSynth buzzSynth;
    [SerializeField] private AudioSource buzzAudio;
    [SerializeField] private float maxBuzzVolume = 0.35f;

    [Header("Player Proximity")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private float fullBuzzDistance = 4f;
    [SerializeField] private float silentBuzzDistance = 18f;
    [SerializeField] private float buzzLerpSpeed = 5f;

    private void Awake()
    {
        if (ps == null)
            ps = GetComponentInChildren<ParticleSystem>();

        if (buzzSynth == null)
            buzzSynth = GetComponent<MosquitoBuzzSynth>();

        if (buzzSynth != null)
            buzzSynth.targetGain = 0f;

        if (buzzAudio == null)
            buzzAudio = GetComponent<AudioSource>();

        if (playerRoot == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerRoot = playerObj.transform;
        }

        if (ps != null)
        {
            var emission = ps.emission;
            baseEmissionRate = GetEmissionRate(emission.rateOverTime);
            currentRate = baseEmissionRate;
        }

        if (buzzAudio != null)
        {
            buzzAudio.loop = true;
            buzzAudio.playOnAwake = true;
            buzzAudio.spatialBlend = 1f;
            buzzAudio.minDistance = 3f;
            buzzAudio.maxDistance = 20f;

            if (!buzzAudio.isPlaying)
                buzzAudio.Play();
        }
    }

    private void Update()
    {
        if (target != null)
            FollowTarget();

        UpdateBuzzVolume();
    }

    void FollowTarget()
    {
        Vector3 desiredPos = target.position;
        desiredPos.y += 1.5f;
        desiredPos.y += Mathf.Sin(Time.time * hoverSpeed) * hoverStrength;

        transform.position = Vector3.MoveTowards(
            transform.position,
            desiredPos,
            moveSpeed * Time.deltaTime
        );
    }

    void ClearTarget()
    {
        target = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            target = other.transform;
    }

    public void Repel()
    {
        if (ps == null) return;

        var emission = ps.emission;
        var rate = emission.rateOverTime;

        rate.constantMin *= 0.5f;
        rate.constantMax *= 0.5f;

        emission.rateOverTime = rate;
    }

    public Vector3 GetEffectPosition()
    {
        if (ps != null)
            return ps.transform.position;

        return transform.position;
    }

    public void UpdateBuzzVolume()
    {
        if (ps == null || buzzSynth == null)
            return;

        var emission = ps.emission;
        currentRate = GetEmissionRate(emission.rateOverTime);

        float emissionNormalized = baseEmissionRate > 0.001f
            ? Mathf.Clamp01(currentRate / baseEmissionRate)
            : 0f;

        float distanceNormalized = 1f;

        if (playerRoot != null)
        {
            float d = Vector3.Distance(playerRoot.position, GetEffectPosition());

            if (d <= fullBuzzDistance)
                distanceNormalized = 1f;
            else if (d >= silentBuzzDistance)
                distanceNormalized = 0f;
            else
                distanceNormalized = Mathf.InverseLerp(silentBuzzDistance, fullBuzzDistance, d);
        }

        float target = emissionNormalized * distanceNormalized * maxBuzzVolume;

        buzzSynth.targetGain = Mathf.Lerp(
            buzzSynth.targetGain,
            target,
            Time.deltaTime * buzzLerpSpeed
        );
    }

    private float GetEmissionRate(ParticleSystem.MinMaxCurve curve)
    {
        return (curve.constantMin + curve.constantMax) * 0.5f;
    }
}
