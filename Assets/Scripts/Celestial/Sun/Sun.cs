using UnityEngine;

public class Sun : MonoBehaviour {

    [SerializeField, HideInInspector] 
    private GameObject sphere;

    private Material material;

    // Emission colors
    public Color color = Color.white;
    public Color emission = Color.white;
    public float intensity = 1.0f;

    // Shader field ids
    private int _emissionColor;
    private int _emissionColorHDRP;
    private int _color;

    private void Awake() {
        _emissionColor = Shader.PropertyToID("_EmissionColor");
        _emissionColorHDRP = Shader.PropertyToID("_EmissiveColor");
        _color = Shader.PropertyToID("_Color");

        Material temp = new Material(GetComponent<Renderer>().material);
        material = new Material(temp);
        GetComponent<Renderer>().material = material;
    }

    private void Update() {
        material.SetColor(_emissionColor, emission * intensity);
        material.SetColor(_emissionColorHDRP, emission * intensity);
        material.SetColor(_color, emission * 0.5f);
    }
}
