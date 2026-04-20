using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MosquitoBuzzSynth : MonoBehaviour
{
    [Header("Base tone")]
    [SerializeField] private float baseFreq = 220f;   // mosquito-ish buzz
    [SerializeField] private float freqJitter = 30f;  // random wobble
    [SerializeField] private float pitchLfoHz = 6f;   // flutter speed

    [Header("Amplitude modulation (swarm wobble)")]
    [SerializeField] private float ampLfoHz = 1.2f;
    [SerializeField] private float ampDepth = 0.6f;

    [Header("Noise mix")]
    [Range(0,1)] [SerializeField] private float noiseMix = 0.5f;

    [Header("Output")]
    [Range(0,1)] public float targetGain = 0.0f; // drive this from emission

    private float phase;
    private float lfoPhasePitch;
    private float lfoPhaseAmp;
    private System.Random rng = new System.Random();
    private float sampleRate;

    private void Awake()
    {
        sampleRate = AudioSettings.outputSampleRate;
    }

    // Called by Unity’s audio thread
    private void OnAudioFilterRead(float[] data, int channels)
    {
        float dt = 1f / sampleRate;

        for (int i = 0; i < data.Length; i += channels)
        {
            // LFOs
            lfoPhasePitch += 2f * Mathf.PI * pitchLfoHz * dt;
            lfoPhaseAmp   += 2f * Mathf.PI * ampLfoHz   * dt;

            float pitchMod = Mathf.Sin(lfoPhasePitch) * 0.5f + 0.5f; // 0..1
            float ampMod   = Mathf.Sin(lfoPhaseAmp);                 // -1..1

            // Slight random walk on frequency
            float jitter = ((float)rng.NextDouble() * 2f - 1f) * freqJitter;
            float freq = baseFreq + jitter + pitchMod * 20f;

            // Oscillator (band-limited-ish via smoothing)
            phase += 2f * Mathf.PI * freq * dt;
            if (phase > 2f * Mathf.PI) phase -= 2f * Mathf.PI;

            float saw = 2f * (phase / (2f * Mathf.PI)) - 1f; // -1..1
            float sine = Mathf.Sin(phase);

            // White noise
            float noise = ((float)rng.NextDouble() * 2f - 1f);

            // Mix tone + noise
            float tone = Mathf.Lerp(sine, saw, 0.6f);
            float sig = Mathf.Lerp(tone, noise, noiseMix);

            // Amplitude modulation (swarm feel)
            float amp = 1f - ampDepth * (0.5f - 0.5f * ampMod); // ~0.7..1.3
            sig *= amp;

            // Apply gain
            float outSample = sig * targetGain;

            // Write to all channels
            for (int c = 0; c < channels; c++)
                data[i + c] = outSample;
        }
    }
}
