using UnityEngine;

public class PlayerShip : ForceEntity {
    
    public float engineAcceleration = 10f;
    public float sensitivity = 2f;
    public bool soft;

    private Rocket[] rockets;

    protected override void Start() {
        base.Start();
        
        rockets = transform.GetComponentsInChildren<Rocket>();
    }
    
    protected override void UpdateInputs() {
        if (Input.GetButtonDown("Soft Controls"))
            soft = !soft;

        float rate = sensitivity * (soft ? 0.5f : 1.0f);
        
        float yaw = -Input.GetAxis("Yaw") * rate;
        float pitch = Input.GetAxis("Pitch") * rate;
        float roll = Input.GetAxis("Roll") * rate;
        
        Quaternion rotation = Quaternion.Euler(yaw, pitch, roll);
        transform.localRotation *= rotation;
    }

    public override Vector3 CalculateThrust() {
        float thrust = Input.GetAxis("Throttle");
        foreach (Rocket rocket in rockets) {
            rocket.current = thrust;
            rocket.OnValidate();
        }
            
        return transform.forward * thrust * engineAcceleration;
    }
}