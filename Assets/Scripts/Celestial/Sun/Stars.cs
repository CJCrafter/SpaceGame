using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class Stars : MonoBehaviour {

    [Min(0)]
    public int stars;
    [Min(0f)]
    public float distance;
    [Range(0, 10)] 
    public int vertexCount;
    public Vector2 sizeRange;
    public Vector2 brightnessRange;
    public Gradient gradient;


    public Shader shader;
    private Mesh mesh;
    private Material material;

    private void Awake() {
        material = new Material(shader);
    }

    public void GenerateMesh() {
        mesh = new Mesh();
        var triangles = new List<int>();
        var vertices = new List<Vector3>();
        var colors = new List<Color>();
        var uvs = new List<Vector2>();

        for (int i = 0; i < stars; i++) {
            Vector3 random = Random.onUnitSphere;
            var (a, b, c, d) = GenerateCircle(random, vertices.Count);
            triangles.AddRange(a);
            vertices.AddRange(b);
            uvs.AddRange(c);
        }

        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0, true);
        mesh.SetUVs(0, uvs);

        var renderer = GetComponent<MeshRenderer>();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    private (int[], Vector3[], Vector2[], Color[]) GenerateCircle(Vector3 direction, int indices) {
        var triangles = new int[vertexCount * 3];
        var vertices = new Vector3[vertexCount + 1];
        var colors = new Color[vertices.Length];
        var uvs = new Vector2[vertices.Length];

        Vector3 center = direction * distance;
        Vector3 axisA = Vector3.Cross((direction == Vector3.up) ? Vector3.forward : Vector3.up, direction);
        Vector3 axisB = Vector3.Cross(axisA, direction);

        float size = Random.Range(sizeRange.x, sizeRange.y);
        float brightness = Random.Range(brightnessRange.x, brightnessRange.y);
        Color color = gradient.Evaluate(Random.value);

        // Set center point
        vertices[0] = center;
        colors[0] = color;
        uvs[0] = new Vector2(brightness, 0f); // y is unused

        for (int i = 0; i < vertexCount; i++) {
            
            // Find point on circle
            float angle = (i / (float) vertexCount) * Mathf.PI * 2;
            Vector3 vertex = center + (axisA * Mathf.Sin(angle) + axisB * Mathf.Cos(angle)) * size;
            vertices[i + 1] = vertex;
            uvs[i + 1] = new Vector2(brightness, 0f); // y is unused
            colors[i + 1] = color;

            triangles[i * 3] = indices;
            triangles[i * 3 + 1] = (i + 1) + indices;
            triangles[i * 3 + 2] = ((i + 1) % vertexCount) + indices; 
        }

        return (triangles, vertices, uvs, colors);
    }
}
