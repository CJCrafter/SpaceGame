using UnityEngine;

public class PlayerShip : Ship {

    public AudioClip clip_switchWeapons;


    protected override void UpdateInputs() {
        
        // * ----- UPDATE ROTATION ----- * //
        float yaw = -Input.GetAxis("Yaw") * sensitivity;
        float pitch = Input.GetAxis("Pitch") * sensitivity;
        float roll = -Input.GetAxis("Roll") * sensitivity;
        
        Quaternion rotation = Quaternion.Euler(yaw, pitch, roll);
        transform.localRotation *= rotation;

        
        // * ----- UPDATE CROSS-HAIR AND FIRE ----- * //
        if (Input.GetButtonDown("Fire"))
            FireWeapon();
        if (Input.GetButtonUp("Fire"))
            StopFireWeapon();
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