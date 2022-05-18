using UnityEngine;

public static class ShaderUtil {

    public static void Dispatch(ComputeShader compute, int kernel, int x, int y = 1, int z = 1) {
        compute.GetKernelThreadGroupSizes(kernel, out uint dx, out uint dy, out uint dz);
        int threadX = Mathf.CeilToInt(x / (float) dx);
        int threadY = Mathf.CeilToInt(y / (float) dy);
        int threadZ = Mathf.CeilToInt(z / (float) dz);
        compute.Dispatch(kernel, threadX, threadY, threadZ);
    }
    
    public static void InitBuffer(ref ComputeBuffer buffer, int count, int stride) {
        if (buffer == null || buffer.count != count || buffer.stride != stride) {
            buffer?.Release();
            buffer = new ComputeBuffer(count, stride);
        }
    }

    public static Texture2D GenerateTextureFromGradient(Gradient gradient, int width) {
        Texture2D texture = null;
        return GenerateTextureFromGradient(gradient, width, ref texture);
    }
    
    public static Texture2D GenerateTextureFromGradient(Gradient gradient, int width, ref Texture2D texture) {
        if (texture == null || texture.height != 1 || texture.width != width) {
            texture = new Texture2D(width, 1)
                {
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
        }

        // No need to reset texture if we have already filled it.
        else {
            return texture;
        }

        // We want to cache the color at each time-step of the gradient. This
        // will cause us to lose some detail, but if "width" is high enough 
        // humans won't see the difference. 
        var pixels = new Color[width];
        for (int i = 0; i < width; i++) {
            float t = i / (float) width;
            Color color = gradient.Evaluate(t);
            pixels[i] = color;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}
