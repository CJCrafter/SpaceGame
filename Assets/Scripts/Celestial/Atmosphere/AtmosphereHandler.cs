using UnityEngine;

[CreateAssetMenu()]
// https://www.desmos.com/calculator/riheoqfknc
public class AtmosphereHandler : ScriptableObject, CameraPost.ICustomPostEffect {

    // Molecular number density of standard atmosphere
    private const float N = 2.504E25f;
    
    public Vector3 wavelengths = new Vector3(440, 550, 680);
    [Min(1)] public float radius = 1.2f; 
    [Min(0)] public int outPoints = 3; 
    [Min(0)] public int inPoints = 3;
    [Range(0, 2)] public float indexOfRefraction = 1.00029f;
    public float scatteringStrength = 4f;
    [Min(0)] public float intensityMultiplier = 1f;
    
    private bool changes = true;
    private Sun sun;
    [HideInInspector] public Material material;
    
    private void OnValidate() {
        changes = true;
    }

    public void UpdateShader(Planet planet) {
        if (material == null) material = new Material(Shader.Find("Celestial/AtmosphereShader"));
        if (sun == null) sun = FindObjectOfType<Sun>();

        Vector3 vector = sun.transform.position - planet.transform.position;
        material.SetVector("_sunDirection", vector.normalized);
        material.SetVector("_planet", planet.transform.position);
        
        material.SetFloat("_atmosphereRadius", planet.radius * radius);
        material.SetFloat("_elevation", planet.terrain.CalculateScaledElevation(planet.elevationBounds.min, planet.radius));
        material.SetInt("_outPoints", outPoints);
        material.SetInt("_inPoints", inPoints);
        material.SetVector("_wavelengths", CalculateBeta(wavelengths));
        material.SetFloat("_sunIntensity", sun.intensity * intensityMultiplier);
        material.SetFloat("_scatteringStrength", scatteringStrength);
    }

    private Vector3 CalculateBeta(Vector3 wavelengths) {
        var a = new Vector3(CalculateBeta(wavelengths.x), CalculateBeta(wavelengths.y), CalculateBeta(wavelengths.z));
        return a;
    }
    
    private float CalculateBeta(float wavelength) {
        const float factor = 8f * Mathf.PI * Mathf.PI * Mathf.PI / 3f / N;
        float temp = indexOfRefraction * indexOfRefraction - 1;
        wavelength /= 1e9f; // nano meters 
        return temp * temp * factor / wavelength / wavelength / wavelength / wavelength;
    }

    public Material GetMaterial() {
        return material;
    }
}