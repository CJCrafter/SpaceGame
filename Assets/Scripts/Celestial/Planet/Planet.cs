using System;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour {
    
    // Some general settings
    [Min(0)] public float radius;
    [Range(0, 5)] public int detail;

    // Since a planet has a bunch of settings, we should divide it into more
    // 'digestible' sections (Also good for saving settings between planets
    // and adding collapsable sections)
    public BiomeHandler biomes;
    public TerrainHandler terrain;
    public AtmosphereHandler atmosphere;


    public void Init() {
        terrain.Update();
        Icosphere.Create(gameObject, detail, 1f);
        //Debug.Log(gameObject.GetComponent<MeshFilter>().mesh.vertexCount);
    }

    public void GenerateMesh() {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        GetComponent<MeshRenderer>().sharedMaterial = biomes.material;

        var oldVertices = mesh.vertices;
        var vertices = new List<Vector3>();

        for (int i = 0; i < mesh.vertexCount; i++) {
            float unscaled = terrain.CalculateUnscaledElevation(oldVertices[i]);
            float elevated = terrain.CalculateScaledElevation(unscaled, radius);

            vertices.Add(oldVertices[i] * elevated);
        }
        
        mesh.SetVertices(vertices);
        mesh.RecalculateNormals();

        var uvs = new List<Vector2>();
        var normals = mesh.normals;
        for (int i = 0; i < mesh.vertexCount; i++) {
            float angle = Vector3.Angle(oldVertices[i], normals[i]);
            uvs.Add(new Vector2(angle, (float) i / mesh.vertexCount));
        }
        
        mesh.SetUVs(0, uvs);
    }

    private void OnValidate() {
        Init();
        GenerateMesh();
    }
}