using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BiomeHandler : ScriptableObject {

    public Gradient gradient;
    public float shoreHeight = 0.1f;
    public float oceanHeight = 0.0f;

    [HideInInspector] public Material material;

    private void OnEnable() {
        //material = new Material(Shader.Find(""));
        material.SetTexture("_Gradient", ShaderUtil.GenerateTextureFromGradient(gradient, 32));
        material.SetFloat("_ShoreHeight", shoreHeight);
        
    }

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
