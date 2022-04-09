using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseFilterRigid : INoiseFilter
{
    NoiseSettings settings;
    Noise noise = new Noise();

    public NoiseFilterRigid(NoiseSettings settings)
    {
        this.settings = settings;
    }

    public float Evaluate(Vector3 point)
    {
        float value = 0f;
        float frequency = settings.baseRoughness;
        float amplitude = 1f;

        // We want the peaks to be extremely detailed, but the valleys
        // should have almost no detail.
        float weight = 1f;

        for (int i = 0; i < settings.layers; i++)
        {
            float temp = 1f - Mathf.Abs(noise.Evaluate(point * frequency + settings.center));
            temp *= temp * weight;
            weight = Mathf.Clamp01(temp * settings.weight);

            value += (temp + 1f) / 2f * amplitude;
            frequency *= settings.roughness;
            amplitude *= settings.persistence;
        }

        return (value - settings.min) * settings.strength;
    }
}
