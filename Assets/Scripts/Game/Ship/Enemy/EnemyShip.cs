using UnityEngine;

public class EnemyShip : Ship {
    
    private ThreatTable threats;
    
    
    
    protected override void UpdateInputs() {
        
        // Rotate towards our target, if we have one
        base.UpdateInputs();
    }

    public override Vector3 CalculateThrust() {
        float thrust = Input.GetAxis("Throttle");
        foreach (Engine rocket in rockets) {
            rocket.current = thrust;
            rocket.OnValidate();
        }
            
        return transform.forward * thrust * engineAcceleration;
    }
}