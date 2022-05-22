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
    public Shader planetShader;

    [HideInInspector] public Planet planet; 
    private Material _material;
    private Material _oceanMaterial;
    private static readonly int _gradient = Shader.PropertyToID("_gradient");
    private static readonly int _shoreHeight = Shader.PropertyToID("_shoreHeight");
    private static readonly int _shoreColor = Shader.PropertyToID("_shoreColor");

    public Material material {
        get {
            if (_material == null)
                _material = new Material(planetShader);

            return _material;
        }
    }

    public void GenerateOcean() {
        if (oceanShader == null)
            return;
        
        if (_oceanMaterial == null) {
            _oceanMaterial = new Material(oceanShader);
        }

        // If the ocean has not been generated yet, generate the ocean sphere.
        if (ocean == null) {
            
            // The ocean may already exist! Lets try to find it first
            foreach (Transform child in planet.transform) {
                if (child.name == "Ocean")
                    ocean = child.gameObject;
            }

            if (ocean == null) {
                Mesh mesh = Icosphere.Create(3, planet.radius * oceanHeight);
                ocean = new GameObject("Ocean");
                ocean.transform.parent = planet.transform;
                ocean.transform.localPosition = Vector3.zero;
                ocean.AddComponent<MeshFilter>();
                ocean.AddComponent<MeshRenderer>();

                ocean.GetComponent<MeshFilter>().sharedMesh = mesh;
                ocean.GetComponent<MeshRenderer>().sharedMaterial = _oceanMaterial;
            }
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
            mesh.RecalculateBounds();
        }
        
        _oceanMaterial.SetColor("_color", oceanColor);
    }

    public void UpdateShader() {
        material.SetTexture(_gradient, ShaderUtil.GenerateTextureFromGradient(gradient, 32));
        material.SetFloat("_planetRadius", planet.radius);
        material.SetFloat(_shoreHeight, shoreHeight);
        material.SetColor(_shoreColor, shoreColor);
    }
}
