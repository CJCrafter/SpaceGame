using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainHandler : ScriptableObject {

    [Min(0f)] 
    public float radius;
    public List<NoiseLayer> layers;
    private INoiseFilter[] filters;

    [System.Serializable]
    public class NoiseLayer {
        public bool hide;
        public bool useMask;
        public NoiseSettings noise;
    }

    public void Update() {
        filters = new INoiseFilter[layers.Count];
        for (int i = 0; i < layers.Count; i++) {
            filters[i] = layers[i].noise.GenerateFilter();
        }
    }

    public float CalculateUnscaledElevation(Vector3 unitSphere) {
        float mask = filters.Length > 0 ? filters[0].Evaluate(unitSphere) : 0f;
        float elevation = filters.Length > 0 && !layers[0].hide ? mask : 0f;

        // Notice we start at 1 since we already handled first layer for masks
        for (int i = 1; i < filters.Length; i++) {
            if (!layers[i].hide) {
                bool useMask = layers[i].useMask;
                elevation += filters[i].Evaluate(unitSphere) * (useMask ? mask : 1f);
            }
        }

        //elevationMinMax.Add(elevation);
        return elevation;
    }

    public float CalculateScaledElevation(float unscaled) {
        return radius * (1f + unscaled);
    }
}
