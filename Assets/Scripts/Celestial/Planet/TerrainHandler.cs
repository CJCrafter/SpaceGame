using System;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainHandler : ScriptableObject {

    [Serializable]
    public struct NoiseSettings {
        public Vector3 center;
        [Range(0, 0.1f)] public float frequency;
        public float amplitude ;
        public uint layers;
        [Range(0, 1)] public float attenuation;
        [Range(0, 1)] public uint rigid;
    }
    
    
    public ComputeShader compute;
    public NoiseSettings shape = new () { frequency = 1f, amplitude = 1f, layers = 5, attenuation = 0.5f };
    public NoiseSettings detail = new () { frequency = 1f, amplitude = 1f, layers = 5, attenuation = 0.5f };
    public NoiseSettings rigid = new () { frequency = 1f, amplitude = 1f, layers = 5, attenuation = 0.5f, rigid = 1 };

    [HideInInspector] public Planet planet;
    private ComputeBuffer vertexBuffer;
    private ComputeBuffer settingsBuffer;
    private ComputeBuffer resultBuffer;
    
    private float[] heights;

    public void Calculate() {
        int count = planet.GetComponent<MeshFilter>().sharedMesh.vertexCount;
        heights = new float[count];
        
        // Do vertex buffer stuff
        var vertices = planet.GetComponent<MeshFilter>().sharedMesh.vertices;
        ShaderUtil.InitBuffer(ref vertexBuffer, count, sizeof(float) * 3);
        vertexBuffer.SetData(vertices);
        
        // Do settings buffer stuff
        var settings = new[]{ shape, detail, rigid };
        ShaderUtil.InitBuffer(ref settingsBuffer, settings.Length, 8 * 4);
        settingsBuffer.SetData(settings);
        
        // Do result buffer stuff
        ShaderUtil.InitBuffer(ref resultBuffer, count, sizeof(float));
        resultBuffer.SetData(heights);

        int kernel = compute.FindKernel("CSMain");
        compute.SetBuffer(kernel, "_vertices", vertexBuffer);
        compute.SetInt("_numVertices", count);
        compute.SetBuffer(kernel, "_settings", settingsBuffer);
        compute.SetInt("_numSettings", settings.Length);
        compute.SetBuffer(kernel, "Result", resultBuffer);
        
        ShaderUtil.Dispatch(compute, kernel, count);
        resultBuffer.GetData(heights);
    }

    public float CalculateUnscaledElevation(int i) {
        if (heights == null || heights.Length != planet.GetComponent<MeshFilter>().sharedMesh.vertexCount)
            Calculate();
        
        return Mathf.Max(0f, heights[i]);
    }

    public float CalculateScaledElevation(float unscaled) {
        return planet.radius * (1f + unscaled);
    }
}
