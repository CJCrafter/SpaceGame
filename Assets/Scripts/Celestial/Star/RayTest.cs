using UnityEngine;
using Color = UnityEngine.Color;

[ExecuteInEditMode]
public class RayTest : MonoBehaviour {
    
    public Color color;
    public float distance;
    [Range(0, 1)] public float longitude;
    [Range(0, 1)] public float latitude;

    public Color intended;
    public Color sampled;
    
    private void Update() {
        Stars stars = FindObjectOfType<Stars>();

        float theta = longitude * 2f * Mathf.PI;
        float phi = latitude * Mathf.PI;
        float x = Mathf.Cos(theta) * Mathf.Sin(phi);
        float y = Mathf.Sin(theta) * Mathf.Sin(phi);
        float z = Mathf.Cos(phi);

        Vector3 direction = new Vector3(x, y, z).normalized;
        Debug.DrawLine(Vector3.zero, direction * distance, color);

        
        Rect rectangle = new Rect((int) (longitude * stars.target.width), (int) (latitude * stars.target.height), 1, 1);
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGB24, false);
        RenderTexture.active = stars.target;
        tex.ReadPixels(rectangle, 0, 0);
        tex.Apply();
        intended = tex.GetPixel(0, 0);



    }
}
