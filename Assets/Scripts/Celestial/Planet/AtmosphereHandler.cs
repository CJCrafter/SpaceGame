using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AtmosphereHandler : ScriptableObject {

    [Range(0f, 1f)] public float atmospherePercentage = 0.05f;
    [Min(0f)] public float atmosphereDensity = 1.225f;
}
