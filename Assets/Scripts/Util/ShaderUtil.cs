using UnityEngine;

public class ShaderUtil
{

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
