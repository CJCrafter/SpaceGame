using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseFilterSimple : INoiseFilter
{
    NoiseSettings settings;
    Noise noise = new Noise();

    public NoiseFilterSimple(NoiseSettings settings)
    {
        this.settings = settings;
    }

    public float Evaluate(Vector3 point)
    {
        float value = 0f;
        float frequency = settings.baseRoughness;
        float amplitude = 1f;

        for (int i = 0; i < settings.layers; i++)
        {
            float temp = noise.Evaluate(point * frequency + settings.center);
            value += (temp + 1f) / 2f * amplitude;
            frequency *= settings.roughness;
            amplitude *= settings.persistence;
        }

        return (value - settings.min) * settings.strength;
    }
}
