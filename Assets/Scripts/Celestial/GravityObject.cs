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
        foreach (GravityObject planet in universe.hasGravity) {
            if (planet.gameObject == gameObject)
                continue;
            
            planet.GetAccelerationAt(transform.position, out Vector3 direction);
            velocity += direction;
        }
    }

    public float GetAccelerationAt(Vector3 position, out Vector3 direction) {
        Vector3 between = transform.position - position;
        float distanceSquared = between.sqrMagnitude;
        float force = Universe.gravitationalConstant * mass / distanceSquared;
        
        between.Normalize();
        between *= force * Time.fixedDeltaTime * universe.timeScale;

        direction = between;
        return force;
    }

    public void ApplyVelocity() {
        transform.position += velocity * Time.fixedDeltaTime * universe.timeScale;
    }
}
