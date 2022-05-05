using System;
using System.Collections.Generic;
using UnityEngine;

public class Universe : MonoBehaviour {

    // Newton's gravitation constant, used in the gravity equation:
    // F = G * (m1 * m2) / (r * r)
    public const float gravitationalConstant = 6.67430E-11f;

    // Do not mutate, only read.
    public List<ForceEntity> hasGravity;
    
    void Start() {
        hasGravity.Clear();

        foreach (ForceEntity obj in FindObjectsOfType<ForceEntity>()) {
            if (obj.hasGravity)
                hasGravity.Add(obj);
        }
    }
}

public class MissingUniverseException : Exception {
}
