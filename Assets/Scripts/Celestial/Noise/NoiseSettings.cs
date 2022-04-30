using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public enum FilterType { SIMPLE, RIGID }
    public FilterType type = FilterType.SIMPLE;

    [Min(0f)]
    public float strength = 1f;
    [Min(0f)]
    public float roughness = 2f;
    public Vector3 center;
    public float min;

    // We want to be able to layer noises together
    // with a decreasing magnitude in order to get fine
    // details without losing the spherical shape
    [UnityEngine.Range(1, 8)]
    public int layers = 1;
    public float baseRoughness = 1f;
    [UnityEngine.Range(0f, 1f)]
    public float persistence = 0.5f;

    [ConditionalHide("type", 1)]
    public float weight = 0.8f;

    public INoiseFilter GenerateFilter()
    {
        switch (type)
        {
            case FilterType.SIMPLE:
                return new NoiseFilterSimple(this);
            case FilterType.RIGID:
                return new NoiseFilterRigid(this);
            default:
                throw new System.Exception("Unknown type: " + type);
        }
    }
}