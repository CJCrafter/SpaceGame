using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class Universe : MonoBehaviour {

    // Newton's gravitation constant, used in the gravity equation:
    // F = G * (m1 * m2) / (r * r)
    public const float gravitationalConstant = 6.67430E-11f;

    // Do not mutate, only read.
    public List<ForceEntity> hasGravity;
    public List<ForceEntity> all;
    
    void Start() {
        hasGravity.Clear();

        foreach (ForceEntity obj in FindObjectsOfType<ForceEntity>()) {
            all.Add(obj);
            if (obj.hasGravity)
                hasGravity.Add(obj);
        }
        
        Events.SHIP_SPAWN.AddListener(e => all.Add(e.ship));
        Events.SHIP_DESTROY.AddListener(e => all.Remove(e.ship));
    }
}

public class MissingUniverseException : Exception {
}
