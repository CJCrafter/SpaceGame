using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Engine : MonoBehaviour {

    [UnityEngine.Range(0f, 1f)] public float current = 1f;
    public ParticleSystem particles;
    public new Light light;
    public AudioClip sound;
    public bool play;
    [SerializeField, Bounds(0f, 1f)] public Range volume;
    [SerializeField, Bounds(0f, 1f)] public Range pitch;

    private AudioSource audio;
    
    private void Start() {
        audio = GetComponent<AudioSource>();
        audio.clip = sound;
        audio.loop = true;
    }
    
    public void OnValidate() {
        current = Mathf.Clamp01(current);

        bool enabled = current > 0.1f;
        if (!enabled) {
            var particlesEmission = particles.emission;
            particlesEmission.enabled = false;
            light.enabled = false;
        }
        else {
            var particlesEmission = particles.emission;
            particlesEmission.enabled = true;
            light.enabled = true;

            light.intensity = Mathf.Lerp(0f, 3.5f, current);
        }
        
        // Handle engine sounds. Only play the sound when the engine is active.
        if (!Application.isPlaying || !play)
            return;

        audio.volume = volume.Lerp(current);
        audio.pitch = pitch.Lerp(current);
        if (current < float.Epsilon)
            audio.Stop();
        else if (!audio.isPlaying)
            audio.Play();
    }
}
