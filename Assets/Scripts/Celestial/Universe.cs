using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Universe is basically just a registry of all GravityObjects. It also calls
 * each GravityObject's 'ApplyGravity' method in the Update() function. 
 */
public class Universe : MonoBehaviour {

    // Newton's gravitation constant, used in the gravity equation:
    // F = G * (m1 * m2) / (r * r)
    public const float gravitationalConstant = 6.67430E-11f;

    // Do not mutate, only read.
    public List<GravityObject> hasGravity;
    public List<GravityObject> usesGravity;

    public float timeScale = 1f;
    
    void Start() {
        hasGravity.Clear();
        usesGravity.Clear();

        foreach (GravityObject obj in FindObjectsOfType<GravityObject>())
        {
            if (obj.hasGravity)
                hasGravity.Add(obj);
            if (obj.usesGravity)
                usesGravity.Add(obj);
        }
    }
    
    void Update()
    {
        foreach (GravityObject obj in hasGravity) {
            if (!obj.gameObject.activeInHierarchy)
                continue;

            obj.ApplyGravity();
        }

        foreach (GravityObject obj in usesGravity) {
            if (!obj.gameObject.activeInHierarchy)
                continue;

            obj.ApplyVelocity();
        }
    }
}

public class MissingUniverseException : Exception {
}
