using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class StarsMesh : MonoBehaviour {

    public int seed;
    [Min(0)]
    public int stars = 100;
    [Min(0f)]
    public float distance = 10000;
    [Range(0, 10)] 
    public int vertexCount = 5;
    public AnimationCurve selector;
    public Vector2 sizeRange = new Vector2(0.5f, 1.5f);
    public Vector2 brightnessRange = new Vector2(1.0f, 2.0f);
    public Gradient gradient;
    public bool debug;
    
    private Mesh mesh;
    public Material material;


    private bool changes = true;
    private static readonly int _Gradient = Shader.PropertyToID("_Gradient");


    public void OnValidate() {
        changes = true;
    }

    private void Update() {
        if (changes) {
            GenerateMesh();
            changes = false;
        }

        if (debug) {
            foreach (Vector3 pos in mesh.vertices) {
                Debug.DrawLine(Vector3.zero, pos, Color.white);
            }
        }
    }

    public void GenerateMesh() {
        Random.InitState(seed);

        mesh = new Mesh();
        var triangles = new List<int>();
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();

        for (int i = 0; i < stars; i++) {
            Vector3 random = Random.onUnitSphere;
            var (a, b, c) = GenerateCircle(random, vertices.Count);
            triangles.AddRange(a);
            vertices.AddRange(b);
            uvs.AddRange(c);
        }

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0, true);

        var renderer = gameObject.GetComponent<MeshRenderer>();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        Texture2D texture = null;
        material.SetTexture(_Gradient, ShaderUtil.GenerateTextureFromGradient(gradient, 128, ref texture));
    }

    private (int[], Vector3[], Vector2[]) GenerateCircle(Vector3 direction, int indices) {
        var triangles = new int[vertexCount * 3];
        var vertices = new Vector3[vertexCount + 1];
        var uvs = new Vector2[vertices.Length];

        Vector3 center = direction * distance;
        Vector3 axisA = Vector3.Cross((direction == Vector3.up) ? Vector3.forward : Vector3.up, direction).normalized;
        Vector3 axisB = Vector3.Cross(axisA, direction).normalized;
        
        StarData star = GenerateSettings();
        float size = star.size;

        // Set center point
        vertices[0] = center;
        uvs[0] = new Vector2(star.brightness, star.color);

        for (int i = 0; i < vertexCount; i++) {
            
            // Find point on circle
            float angle = (i / (float) vertexCount) * Mathf.PI * 2;
            Vector3 vertex = center + (axisA * Mathf.Sin(angle) + axisB * Mathf.Cos(angle)) * size;
            vertices[i + 1] = vertex;
            uvs[i + 1] = uvs[0]; // The brightness/color of each star doesn't change per vertex

            triangles[i * 3 + 0] = indices;
            triangles[i * 3 + 1] = (i + 1) + indices;
            triangles[i * 3 + 2] = ((i + 1) % vertexCount + 1) + indices; 
        }

        return (triangles, vertices, uvs);
    }

    private StarData GenerateSettings() {
        float val = selector.Evaluate(Random.value);

        float time = val;
        float brightness = val * (brightnessRange.y - brightnessRange.x) + brightnessRange.x;
        float size = val * (sizeRange.y - sizeRange.x) + sizeRange.x;

        return new StarData
            {
                color = time,
                brightness = brightness,
                size = size
            };
    }

    private struct StarData {
        internal float color;
        internal float brightness;
        internal float size;
    }
}
