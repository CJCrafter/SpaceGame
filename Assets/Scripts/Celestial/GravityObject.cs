using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[ExecuteInEditMode]
public class GravityObject : MonoBehaviour {

    // Inspector arguments
    public bool usesGravity;
    public bool hasGravity;
    public float mass;
    
    public Vector3 velocity;
    private Universe universe;

    public void Start() {

        universe = FindObjectOfType<Universe>();
        if (universe == null)
            throw new MissingUniverseException();

        if (mass < 5.0f) 
            Debug.Log("Low mass value '" + mass + "' detected for " + this);
    }

    public void ApplyGravity() {
        Universe universe = FindObjectOfType<Universe>();
        foreach (var planet in universe.hasGravity) {
            Vector3 direction = planet.transform.position - transform.position;

            float distanceSquared = direction.sqrMagnitude;
            if (distanceSquared == 0.0f)
                continue;

            float force = Universe.gravitationalConstant * (mass * planet.mass) / distanceSquared;

            direction.Normalize();
            direction *= force / mass * Time.deltaTime * universe.timeScale;
            velocity += direction;
        }
    }

    public void ApplyVelocity() {
        Universe universe = FindObjectOfType<Universe>();
        transform.position += velocity * Time.deltaTime * universe.timeScale;
    }
}
