using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour {
    
    [Range(0f, 1f)] public float current = 1f;
    public ParticleSystem particles;
    public new Light light;

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
    }
}
