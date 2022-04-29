using System;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour {

    private const int MIN_DETAIL = 0;
    private const int MAX_DETAIL = 6;
    
    // Some general settings
    [Min(0)] public float radius;
    [Min(0)] public float lodMultiplier; 
    
    // Since a planet has a bunch of settings, we should divide it into more
    // 'digestible' sections (Also good for saving settings between planets
    // and adding collapsable sections)
    public BiomeHandler biomes;
    public TerrainHandler terrain;
    public AtmosphereHandler atmosphere;

    // Consider a planet with radius 'r', and a camera with a distance 'd' to
    // that planet. When d < 10r, there should be a lot of detail on the
    // planet. As d gets bigger and bigger, the detail on the planet must lower
    // to save resources. 
    [SerializeField, HideInInspector] 
    private Mesh[] LODS;
    private Camera camera;
    public readonly MinMax elevationBounds = new MinMax();
    
    public void Init() {
        terrain.Update();

        if (true || LODS == null || LODS.Length != MAX_DETAIL - MIN_DETAIL || LODS.Length == 0 || LODS[0] == null) {
            Debug.Log("Regenerating " + gameObject.name + " mesh from: " + LODS);
            if (gameObject.GetComponent<MeshRenderer>() == null) gameObject.AddComponent<MeshRenderer>();
            if (gameObject.GetComponent<MeshFilter>() == null) gameObject.AddComponent<MeshFilter>();
            LODS = new Mesh[MAX_DETAIL - MIN_DETAIL];

            for (int i = 0; i < MAX_DETAIL - MIN_DETAIL; i++) {
                LODS[i] = Icosphere.Create(MIN_DETAIL + i, 1.0f);
                LODS[i].name = gameObject.name + " LOD " + (MIN_DETAIL + 1);
            }
        }
    }

    public void GenerateMeshes() {
        
        foreach (Mesh mesh in LODS)
            GenerateMesh(mesh);
        
        // When the application not running, we should save on resources
        // to make editing the planet less painful
        if (true || !Application.isPlaying) {
            Debug.Log("ABI SMELLS " + LODS[LODS.Length - 1].name);
            GetComponent<MeshFilter>().sharedMesh = LODS[LODS.Length - 1];
            GetComponent<MeshRenderer>().sharedMaterial = biomes.material;
        }
    }

    private void GenerateMesh(Mesh mesh) {
        var oldVertices = mesh.vertices;
        var vertices = new List<Vector3>();
        var elevations = new float[mesh.vertexCount];
        
        for (int i = 0; i < mesh.vertexCount; i++) {
            oldVertices[i] = oldVertices[i].normalized;
            float unscaled = terrain.CalculateUnscaledElevation(oldVertices[i]);
            float elevated = terrain.CalculateScaledElevation(unscaled, radius);

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
    }

    public int GetCurrentLOD() {
        float distance = MathUtil.Distance(camera.transform.position, transform.position);
        float maxDistance = radius * lodMultiplier;
        distance = Mathf.Clamp(distance, 0, maxDistance);
        return Mathf.RoundToInt(MathUtil.Remap(distance, 0, maxDistance, MIN_DETAIL, MAX_DETAIL));
        
    }
}