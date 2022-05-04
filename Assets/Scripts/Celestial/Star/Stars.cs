using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Stars : MonoBehaviour {
    
    public int seed;
    [Range(1, 8)] public int resolution;
    [Min(0)] public int stars = 6000;
    [Min(0f)] public float distance = 100000;
    public AnimationCurve selector;
    [Bounds(0f, 1f)] public Range brightness;
    [Bounds(0f, 1000f)] public Range size;
    public Gradient gradient;

    // Used to handle 'white dwarf' and 'red giant' stars
    [Range(0f, 1f)] public float whiteDwarfSize;
    [Range(0f, 1f)] public float whiteDwarfChance;
    [Range(0.5f, 1f)] public float whiteDwarfMultiplier;
    [Range(0f, 1f)] public float redGiantSize;
    [Range(0f, 1f)] public float redGiantChance;
    [Min(1f)] public float redGiantMultiplier;
    
    public ComputeShader compute;
    [SerializeReference, HideInInspector] public RenderTexture target;
    private ComputeBuffer buffer;
    private Material skybox;

    // * ----- PROPERTY CACHE ----- * // 
    private static readonly int _mainTex = Shader.PropertyToID("_MainTex");

    private void Awake() {
        
        if (target == null) {
            Generate();
        }
        else {
            Init();
            skybox.SetTexture(_mainTex, target);
            RenderSettings.skybox = skybox;
        }
    }

    private void Init() {
        if (target == null || target.width != Screen.width * resolution || target.height != Screen.height * resolution) {
            if (target != null)
                target.Release();

            target = new RenderTexture(Screen.width * resolution, Screen.height * resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();
        }

        if (buffer == null || buffer.count != stars) {
            buffer?.Release();
            buffer = new ComputeBuffer(stars, 4 * 6);
        }

        if (skybox == null) {
            skybox = new Material(Shader.Find("Skybox/Panoramic"));
        }
    }

    public void Generate() {
        float startTime = Time.realtimeSinceStartup;
        Debug.Log("Generating star box");
        Init();
        
        Random.InitState(seed);
        List<StarData> stars = new List<StarData>(this.stars);
        for (int i = 0; i < this.stars; i++) {
            stars.Add(GenerateSettings());    
        }

        buffer.SetData(stars);
        
        Camera camera = Camera.current == null ? Camera.main : Camera.current;
        if (camera == null) {
            Debug.LogWarning("Null camera stars");
            return;
        }

        int kernel = compute.FindKernel("CSMain");
        compute.SetTexture(kernel, "Result", target);
        int threadX = Mathf.CeilToInt(resolution * Screen.width / 8f);
        int threadY = Mathf.CeilToInt(resolution * Screen.height / 8f);
        
       
        compute.SetBuffer(kernel, "_stars", buffer);
        compute.SetInt("_starCount", this.stars);
        compute.SetTexture(kernel, "_gradient", ShaderUtil.GenerateTextureFromGradient(gradient, 128));
        compute.SetMatrix("_cameraToWorld", camera.cameraToWorldMatrix);
        compute.SetMatrix("_cameraInverseProjection", camera.projectionMatrix.inverse);

        compute.Dispatch(kernel, threadX, threadY, 1);
        skybox.SetTexture(_mainTex, target);

        //test.targetTexture = target;
        RenderSettings.skybox = skybox;
        Debug.Log("Took " + (Time.realtimeSinceStartup - startTime) + "s to calculate stars");
    }

    public void GenerateFile() {
        Texture2D png = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        RenderTexture.active = target;
        png.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
        png.Apply();
        //Destroy(png); // executes on next frame
        byte[] bytes = png.EncodeToPNG();
        System.IO.File.WriteAllBytes("C:\\Users\\colli\\Desktop\\Code\\SpaceGame\\Assets\\Scripts\\Celestial\\Star\\img.png", bytes);
    }


    private void OnDisable() {
        buffer?.Release();
        if (target != null)
            target.Release();
    }

    private StarData GenerateSettings() {
        float val = selector.Evaluate(Random.value);

        float brightness = this.brightness.Lerp(Random.value);
        float radius = size.Lerp(val);
        
        // Handle 'white dwarf' and 'red giant' special cases. Big
        // stars have a small chance to become a red giant, and small
        // stars have a medium chance to become a white dwarf. Red giants
        // additionally have their size multiplied since they tend to be
        // MUCH LARGER then average stars. 
        float time = val;
        if (radius < whiteDwarfSize && Random.value < whiteDwarfChance) {
            radius *= Random.Range(0.5f, whiteDwarfMultiplier);
            time = Random.Range(0.45f, 0.55f); // white
        }
        else if (radius > redGiantSize && Random.value < redGiantChance) {
            radius *= Random.Range(1f, redGiantMultiplier);
            time = Random.Range(0f, 0.15f); // red
        }
        
        return new StarData
            {
                position = Random.onUnitSphere * distance,
                time = time,
                brightness = brightness,
                radius = radius
            };
    }

    private struct StarData {
        internal Vector3 position;
        internal float time;
        internal float brightness;
        internal float radius; 
    }
}