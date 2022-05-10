using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class ShowOrbit : MonoBehaviour {
    
    private ForceEntity root;
    private LineRenderer line;
    private Vector3[] _points;
    private bool _useGL;

    private void Start() {
        root = GetComponent<ForceEntity>();
        line = GetComponent<LineRenderer>();
    }

    public Vector3[] points {
        get => _points;
        set {
            _points = value;
            orbit = MathUtil.SquareDistance(_points[0], _points[^1]) < 25f;
            sampleTime = Time.timeSinceLevelLoad;
            line.sharedMaterial ??= new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            //line.startWidth = line.endWidth = Vector3.Distance(root.transform.position, Camera.main.transform.position) / 1000f;
            line.positionCount = points.Length;
            line.SetPositions(points);
        }
    }

    public bool orbit { get; private set; }

    public bool escape => !orbit;

    public bool useGL {
        get => _useGL;
        set {
            _useGL = value;
            line.enabled = !useGL && enabled;
        }
    }

    public float sampleTime { get; private set; }
    
    internal void Render() {
        if (!enabled || !useGL || points.Length < 2)
            return;

        for (var i = 1; i < points.Length; i++) {
            Color color = line.colorGradient.Evaluate(i / (float) points.Length);
            if (i % 2 == 0)
                color = new Color(1f - color.r, 1f - color.g, 1f - color.b);
            Debug.DrawLine(points[i - 1], points[i], color);
        }
    }
}