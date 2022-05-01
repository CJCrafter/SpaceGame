using UnityEngine;

public class RayMarching : MonoBehaviour {

    public ComputeShader compute;
    public Texture skybox;
    public Light light1;
    
    private RenderTexture target;
    private uint samples;
    private Material addMaterial;
    private static readonly int Sample = Shader.PropertyToID("_Sample");

    private void Update() {
        if (transform.hasChanged) {
            samples = 0;
            transform.hasChanged = false;
        }

        if (light1.transform.hasChanged) {
            samples = 0;
            light1.transform.hasChanged = false;
        }
    }
    
    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        Render(dest);
    }

    private void Render(RenderTexture destination) {
        InitRenderTexture();
        
        compute.SetTexture(0, "Result", target);
        int threadX = Mathf.CeilToInt(Screen.width / 8f);
        int threadY = Mathf.CeilToInt(Screen.height / 8f);
        
        // Handle actual rendering calculations 
        UpdateShader();
        compute.Dispatch(0, threadX, threadY, 1);
        if (addMaterial == null)
            addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        addMaterial.SetFloat(Sample, samples);
        Graphics.Blit(target, destination, addMaterial);
        samples++;
    }

    private void InitRenderTexture() {
        if (target == null || target.width != Screen.width || target.height != Screen.height) {
            if (target != null)
                target.Release();

            target = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();
        }
    }

    private void UpdateShader() {
        Camera camera = Camera.current == null ? Camera.main : Camera.current;
        if (camera == null) {
            Debug.LogWarning("Null camera ray march");
            return;
        }

        Vector3 l = light1.transform.forward;
        compute.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, light1.intensity));
        compute.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
        compute.SetMatrix("_CameraToWorld", camera.cameraToWorldMatrix);
        compute.SetMatrix("_CameraInverseProjection", camera.projectionMatrix.inverse);
        compute.SetTexture(0, "_SkyboxTexture", skybox);
    }
}
