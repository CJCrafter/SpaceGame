using UnityEngine;

public class CelestialEntity : ForceEntity {

    public override void CalculateForces() {
        foreach (Vector3 ray in CalculateGravity()) {
            body.AddForce(ray, ForceMode.Impulse);
        }
    }
    
}