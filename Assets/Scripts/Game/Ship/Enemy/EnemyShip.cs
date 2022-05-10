using UnityEngine;

[RequireComponent(typeof(ThreatTable))]
public class EnemyShip : Ship {
    
    private ThreatTable threats;
    private ThreatTable.ShootData shoot;
    
    protected override void Start() {
        base.Start();

        hotbar = HOTBAR_LASER + 1;
        threats = GetComponent<ThreatTable>();
    }

    protected override void UpdateInputs() {
        shoot = threats.GetTargetLocation();

        bool hasTarget = threats.HasTarget();
        fire = hasTarget && Vector3.Dot(shoot.Ray(transform.position).direction, transform.forward) > 0f;
        //Debug.DrawRay(origin, direction, Time.frameCount % 2 == 0 ? Color.white : Color.red, 0.25f);

        if (hasTarget) {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, GetShootDirection(transform.position), sensitivity);
        }

        base.UpdateInputs();
    }

    public override Quaternion GetShootDirection(Vector3 origin) {
        Vector3 direction = shoot.Ray(origin).direction;
        Debug.DrawRay(origin, direction, Color.cyan, 1f);
        return Quaternion.LookRotation(shoot.Ray(origin).direction);
    }

    public override Vector3 CalculateThrust() {
        float thrust = Input.GetAxis("Throttle");
        foreach (Engine rocket in rockets) {
            rocket.current = thrust;
            rocket.OnValidate();
        }
            
        return transform.forward * thrust * engineAcceleration;
    }

    public override ForceEntity GetTarget() {
        return threats.threat.entity;
    }
}