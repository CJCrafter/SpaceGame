using UnityEngine;

[CreateAssetMenu()]
public class BiomeHandler : ScriptableObject {

    public Gradient gradient;
    public float shoreHeight = 0.1f;
    public Color shoreColor = Color.yellow;
    public float oceanHeight = 0.0f;
    public Color oceanColor = Color.blue;
    public GameObject ocean;
    public Shader oceanShader; 

    [HideInInspector] public Planet planet; 
    private Material _material;
    private Material _oceanMaterial;
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

    public void GenerateOcean(bool reset) {

        if (_oceanMaterial == null || reset) {
            _oceanMaterial = new Material(oceanShader);
        }

        // If the ocean has not been generated yet, generate the ocean sphere.
        if (ocean == null || reset) {
            Mesh mesh = Icosphere.Create(3, planet.radius * oceanHeight);
            ocean = new GameObject("Ocean");
            ocean.transform.parent = planet.transform;
            ocean.transform.localPosition = Vector3.zero;
            ocean.AddComponent<MeshFilter>();
            ocean.AddComponent<MeshRenderer>();
            
            ocean.GetComponent<MeshFilter>().sharedMesh = mesh;
            ocean.GetComponent<MeshRenderer>().sharedMaterial = _oceanMaterial;
        }
        
        // Make sure to update ocean height
        else {
            Mesh mesh = ocean.GetComponent<MeshFilter>().sharedMesh;
            var vertices = mesh.vertices;
            
            for (int i = 0; i < vertices.Length; i++) {
                vertices[i].Normalize();
                vertices[i] *= planet.radius * oceanHeight;
            }

            mesh.vertices = vertices;
        }
        
        _oceanMaterial.SetColor("_color", oceanColor);
    }

    public void UpdateShader() {
        material.SetTexture(_gradient, ShaderUtil.GenerateTextureFromGradient(gradient, 32));
        material.SetFloat(_shoreHeight, shoreHeight);
        material.SetColor(_shoreColor, shoreColor);
    }
}
