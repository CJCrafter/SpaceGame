public class MinMax {
    
    public float min { get; private set; }
    public float max { get; private set; }

    public MinMax() {
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