using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TempStars : MonoBehaviour {

    // Main star attributes
    public int seed;
    [Bounds(0f, 1f)] public Range brightness;
    [Bounds(220f, 340f)] public Range size;
    [Min(0f)] public float distance = 100000f;
    [Min(1)] public int numStars = 6000;
    [Min(4)] public int numVertices = 5;
    public Gradient gradient; 
    
    // Used to handle 'white dwarf' and 'red giant' stars
    [Range(0f, 1f)] public float whiteDwarfSize = 0.1f;
    [Range(0f, 1f)] public float whiteDwarfChance = 0.33f;
    [Range(0f, 1f)] public float whiteDwarfMultiplier = 0.75f;
    [Range(0f, 1f)] public float redGiantSize = 0.9f;
    [Range(0f, 1f)] public float redGiantChance = 0.05f;
    [Min(1f)] public float redGiantMultiplier = 5f;
    
    // Other misc
    public Material material;
    private static readonly int _gradient = Shader.PropertyToID("_Gradient");

    public void OnValidate() {
        Regenerate();
    }

    public void Regenerate() {
        Debug.Log("Regenerating Stars");
        Random.InitState(seed);

        var triangles = new List<int>();
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();

        for (int i = 0; i < numStars; i++) {
            StarData star = GenerateRandom();
            var (a, b, c) = GenerateCircle(star, vertices.Count);
            triangles.AddRange(a);
            vertices.AddRange(b);
            uvs.AddRange(c);
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0, true);
        mesh.SetUVs(0, uvs);

        var renderer = gameObject.GetComponent<MeshRenderer>();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        
        Texture2D texture = ShaderUtil.GenerateTextureFromGradient(gradient, 64);
        material.SetTexture(_gradient, texture);
    }

    private (int[], Vector3[], Vector2[]) GenerateCircle(StarData star, int offset) {
        var triangles = new int[numVertices * 3];
        var vertices = new Vector3[numVertices + 1];
        var uvs = new Vector2[vertices.Length];

        Vector3 dummy = star.position.normalized == Vector3.up ? Vector3.forward : Vector3.up;
        Vector3 axisA = Vector3.Cross(star.position, dummy).normalized;
        Vector3 axisB = Vector3.Cross(axisA, star.position).normalized;

        // Set initial values. Note that we don't us 'uvs' as uv coordinates,
        // instead we use them to store the star's brightness and color.
        vertices[0] = star.position;
        uvs[0] = new Vector2(star.brightness, star.time);

        for (int i = 0; i < numVertices; i++) {

            float angle = (i / (float)numVertices) * Mathf.PI * 2f;
            Vector3 vertex = star.position + (axisA * Mathf.Sin(angle) + axisB * Mathf.Cos(angle)) * star.radius;
            vertices[i + 1] = vertex;
            uvs[i + 1] = uvs[0]; // Brightness and color are constant across the star

            // Setup triangles, drawing from the origin to some point to another point.
            triangles[i * 3] = offset;
            triangles[i * 3 + 1] = offset + i + 1;
            triangles[i * 3 + 2] = offset + (i + 1) % numVertices + 1;
        }

        return (triangles, vertices, uvs);
    }
    
    

    private StarData GenerateRandom() {
        float val = Random.value;
        
        float brightness = this.brightness.Lerp(val);
        float radius = size.Lerp(val);

        // Handle 'white dwarf' and 'red giant' special cases. Big
        // stars have a small chance to become a red giant, and small
        // stars have a medium chance to become a white dwarf. Red giants
        // additionally have their size multiplied since they tend to be
        // MUCH LARGER then average stars. 
        float time = val;
        if (val < whiteDwarfSize && Random.value < whiteDwarfChance) {
            radius *= whiteDwarfMultiplier;
            time = 0.5f; // white
        }
        else if (val > redGiantSize && Random.value < redGiantChance) {
            radius *= redGiantMultiplier;
            time = 0.0f; // red
        }
        
        return new StarData()
        {
            position = Random.onUnitSphere * distance,
            time = time,
            brightness = brightness,
            radius = radius
        };
    }
    
    private class StarData {
        internal Vector3 position;
        internal float time; // Number between 0 and 1 to determine color
        internal float brightness;
        internal float radius;
    }

}