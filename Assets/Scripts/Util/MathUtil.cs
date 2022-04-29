using UnityEngine;

public class MathUtil {

    public static float SquareDistance(Vector3 a, Vector3 b) {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        float dz = a.z - b.z;

        return dx * dx + dy * dy + dx * dz;
    }
    
    public static float Distance(Vector3 a, Vector3 b) {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        float dz = a.z - b.z;

        return Mathf.Sqrt(dx * dx + dy * dy + dx * dz);
    }
    
    public static float Remap(float t, float oldMin, float oldMax, float min, float max) {
        return ((t - oldMin) / (oldMax - oldMin)) * (max - min) + min;
    }
}