using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class Shield : MonoBehaviour {
    
    public List<AudioClip> damageSounds;
    [Min(0f)] public float maxStrength = 100f;
    [Min(0f)] public float regenRate = 1f;
    [Min(0f)] public float regenDelay = 5f;
    [Min(0f)] public float forwardStrength;
    [Min(0f)] public float aftStrength;
    [Min(0f)] public float shieldFade = 3.0f;

    private Ship ship;
    private ParticleSystem particles;
    private Color shieldColor;
    private float lastHitTime;
    private new AudioSource audio;
    

    private void Start() {
        ship = transform.parent.gameObject.GetComponent<Ship>();
        particles = GetComponent<ParticleSystem>();
        shieldColor = particles.main.startColor.color;
        audio = GetComponent<AudioSource>();
        if (ship == null)
            Debug.LogWarning("Shield is not on a ship?");
    }
    
    private void FixedUpdate() {
        
        // Change the opacity of the shield based on the current strength, and
        // the time since last hit. We want the shield to fade out when not being
        // used to help show the player they are not in danger.
        float deltaTime = Time.timeSinceLevelLoad - lastHitTime;
        if (deltaTime < shieldFade) {
            float shieldPower = Math.Min(forwardStrength, aftStrength) / maxStrength;
            float alpha = MathUtil.Remap(deltaTime, 0, shieldFade, 0, shieldPower);
            var particlesMain = particles.main;
            var startColor = particlesMain.startColor;
            startColor.color = new Color(shieldColor.r, shieldColor.g, shieldColor.b, alpha);
            particlesMain.startColor = startColor;
        }

        // Regenerate shields over time. We should also consider playing a
        // shield regeneration sound to let the player know the condition.
        if (deltaTime > regenDelay) {
            forwardStrength = Mathf.MoveTowards(forwardStrength, maxStrength, regenRate * Time.fixedDeltaTime);
            aftStrength = Mathf.MoveTowards(aftStrength, maxStrength, regenRate * Time.fixedDeltaTime);
        }
    }

    public float Damage(bool forward, float damage) {
        float shieldStrength = forward ? forwardStrength : aftStrength;
        
        // When the shield has no strength left, simply return the damage
        if (shieldStrength <= float.Epsilon) 
            return damage;

        lastHitTime = Time.timeSinceLevelLoad;
        
        shieldStrength -= damage;
        float spillOver = 0f;
        if (shieldStrength < 0f) {
            spillOver = -shieldStrength;
            shieldStrength = 0;
        }

        // Update the strength left in the forward or aft shield. 
        if (forward)
            forwardStrength = shieldStrength;
        else
            aftStrength = shieldStrength;
        
        // Play audio effects
        float pitch = Math.Min(forwardStrength, aftStrength) / maxStrength;
        audio.clip = damageSounds[Random.Range(0, damageSounds.Count)];
        audio.pitch = pitch;
        audio.Play();
        
        return spillOver;
    }

}