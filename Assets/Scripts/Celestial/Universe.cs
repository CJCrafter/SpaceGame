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

        foreach (ForceEntity obj in FindObjectsOfType<ForceEntity>())
            AddEntity(obj);

        Events.ENTITY_SPAWN.AddListener(e => AddEntity(e.entity));
        Events.ENTITY_DESTROY.AddListener(e => RemoveEntity(e.entity));
    }

    public void AddEntity(ForceEntity entity) {
        all.Add(entity);
        if (entity.hasGravity)
            hasGravity.Add(entity);
    }

    public void RemoveEntity(ForceEntity entity) {
        all.Remove(entity);
        hasGravity.Remove(entity);
    }
}

public class MissingUniverseException : Exception {
}
