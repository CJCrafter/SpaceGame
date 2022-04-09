using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BiomeHandler : ScriptableObject {

    public Material material;
    public Gradient ocean;
    public Biome[] biomes;
    public NoiseSettings biomeNoise;
    public float noiseOffset;
    public float noiseStrength;
    [Range(0f, 1f)]
    public float blendAmount;

    [System.Serializable]
    public class Biome
    {
        public Gradient gradient;
        public Color tint;
        [Range(0f, 1f)]
        public float startHeight;
        [Range(0f, 1f)]
        public float tintPercent;
    }

    public BiomeHandler() {
    }
}
