using UnityEngine;

public class PlayerShip : Ship {
    
    public bool soft;
    public float crosshairSensitivity;

    private Vector2 crosshair;
    
    
    protected override void UpdateInputs() {
        
        // * ----- UPDATE ROTATION ----- * //
        if (Input.GetButtonDown("Soft Controls"))
            soft = !soft;

        float rate = sensitivity * (soft ? 0.5f : 1.0f);
        
        float yaw = -Input.GetAxis("Yaw") * rate;
        float pitch = Input.GetAxis("Pitch") * rate;
        float roll = Input.GetAxis("Roll") * rate;
        
        Quaternion rotation = Quaternion.Euler(yaw, pitch, roll);
        transform.localRotation *= rotation;

        
        // * ----- UPDATE CROSS-HAIR AND FIRE ----- * //
        if (Input.GetButton("Fire")) {
            FireWeapon();
        }
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