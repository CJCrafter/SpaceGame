using UnityEngine;

[System.Serializable]
public class MinMax {

    [SerializeField] private float _min;
    [SerializeField] private float _max;

    public float min { get => _min; private set => _min = value; }
    public float max { get => _max; private set => _max = value; }

    public MinMax() {
        Clear();
    }

    public void Clear() {
        min = float.PositiveInfinity;
        max = float.NegativeInfinity;
    }

    public void Add(float value) {
        if (value < min)
            min = value;
        if (value > max)
            max = value;
    }
}