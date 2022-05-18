using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour {

    // Some general settings
    [Min(0)] public float radius;
    
    // Since a planet has a bunch of settings, we should divide it into more
    // 'digestible' sections (Also good for saving settings between planets
    // and adding collapsable sections)
    public BiomeHandler biomes;
    public TerrainHandler terrain;
    public AtmosphereHandler atmosphere;

    [SerializeField, HideInInspector] private Mesh mesh; 
    private new Camera camera;
    [SerializeField, HideInInspector] public MinMax elevationBounds = new MinMax();
    
    public void Init() {
        if (GetComponent<MeshRenderer>() == null) gameObject.AddComponent<MeshRenderer>();
        if (GetComponent<MeshFilter>() == null) gameObject.AddComponent<MeshFilter>();
        if (GetComponent<MeshCollider>() == null) gameObject.AddComponent<MeshCollider>();
        
        if (mesh == null)
            GenerateMeshes(true, true);

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial = biomes.material;
    }

    public void Start() {
        if (atmosphere == null)
            return;

        // The editor also sets these, but let's just make sure that
        // our handlers know which planet they are used for. 
        atmosphere.planet = this;
        terrain.planet = this;
        
        atmosphere.UpdateShader();
        CameraPost post = FindObjectOfType<CameraPost>();
        post.materials.Add(atmosphere.material);
    }

    public void Update() {
        atmosphere.UpdateShader();
    }

    public void GenerateMeshes(bool recalculateNoise, bool recalculateAll) {
        elevationBounds.Clear();
        if (recalculateNoise)
            terrain.Calculate();

        GenerateMesh(recalculateAll);
    }

    public void GenerateMesh(bool recalculateAll) {
        if (mesh == null || recalculateAll)
            mesh = GetComponent<MeshFilter>().sharedMesh = Icosphere.Create(5, 1);
        
        var oldVertices = mesh.vertices;
        var vertices = new List<Vector3>();
        var elevations = new float[mesh.vertexCount];
        
        for (int i = 0; i < mesh.vertexCount; i++) {
            oldVertices[i] = oldVertices[i].normalized;
            float unscaled = terrain.CalculateUnscaledElevation(i);
            float elevated = terrain.CalculateScaledElevation(unscaled);

            elevations[i] = unscaled;
            vertices.Add(oldVertices[i] * elevated);
            elevationBounds.Add(unscaled);
        }
        
        mesh.SetVertices(vertices);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        var uvs = new List<Vector2>();
        var normals = mesh.normals;
        for (int i = 0; i < mesh.vertexCount; i++) {
            float angle = Vector3.Angle(oldVertices[i], normals[i]) / 90f;
            uvs.Add(new Vector2(angle, elevations[i]));
        }
        
        mesh.SetUVs(0, uvs);
        mesh.Optimize();
    }
}