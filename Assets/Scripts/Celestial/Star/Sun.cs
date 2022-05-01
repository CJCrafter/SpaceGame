using UnityEngine;

public class Sun : MonoBehaviour {

    public Material material;

    // Emission colors
    public Color emission = Color.white;
    public float intensity = 1.0f;

    // Shader field ids
    private static readonly int _emissionColor = Shader.PropertyToID("_EmissionColor");
    
    private void OnValidate() {
        material.SetColor(_emissionColor, emission * intensity);
        material.SetColor("_EmissiveColor", emission * intensity);
        material.SetColor("_Color", emission * 0.5f);
    }
}
