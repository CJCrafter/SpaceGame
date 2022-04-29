using UnityEngine;

[CreateAssetMenu()]
public class BiomeHandler : ScriptableObject {

    public Gradient gradient;
    public float shoreHeight = 0.1f;
    public Color shoreColor = Color.yellow;
    public float oceanHeight = 0.0f;

    private Material _material;
    private static readonly int _gradient = Shader.PropertyToID("_Gradient");
    private static readonly int _shoreHeight = Shader.PropertyToID("_ShoreHeight");
    private static readonly int _shoreColor = Shader.PropertyToID("_ShoreColor");

    public Material material {
        get {
            if (_material == null)
                _material = new Material(Shader.Find("Celestial/PlanetShader"));

            return _material;
        }
    }

    public void UpdateShader() {
        material.SetTexture(_gradient, ShaderUtil.GenerateTextureFromGradient(gradient, 32));
        material.SetFloat(_shoreHeight, shoreHeight);
        material.SetColor(_shoreColor, shoreColor);
    }
}
