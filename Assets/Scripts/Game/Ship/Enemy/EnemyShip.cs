using UnityEngine;

[RequireComponent(typeof(ThreatTable))]
public class EnemyShip : Ship {
    
    private ThreatTable threats;
    
    protected override void Start() {
        base.Start();
        
        threats = GetComponent<ThreatTable>();
    }

    protected override void UpdateInputs() {
        
        // Rotate towards our target, if we have one
        base.UpdateInputs();

        ThreatTable.ShootData shoot = threats.GetTargetLocation();
        (Vector3 origin, Vector3 direction) = shoot.Ray(transform.position);
        Debug.DrawRay(origin, direction, Time.frameCount % 2 == 0 ? Color.white : Color.red, 0.25f);
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