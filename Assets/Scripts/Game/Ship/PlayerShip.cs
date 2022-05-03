using UnityEngine;
using UnityEngine.UI;

public class PlayerShip : Ship {

    public AudioClip clip_switchWeapons;
    public Image[] hotbarImages;

    protected override void UpdateInputs() {
        
        // * ----- UPDATE ROTATION ----- * //
        float yaw = -Input.GetAxis("Yaw") * sensitivity;
        float pitch = Input.GetAxis("Pitch") * sensitivity;
        float roll = -Input.GetAxis("Roll") * sensitivity;
        
        Quaternion rotation = Quaternion.Euler(yaw, pitch, roll);
        transform.localRotation *= rotation;

        
        // * ----- UPDATE CROSS-HAIR AND FIRE ----- * //
        if (Input.GetButtonDown("Hotbar 1"))
            hotbar = 1;
        else if (Input.GetButtonDown("Hotbar 2"))
            hotbar = 2;

        int scroll = Mathf.RoundToInt(Input.GetAxisRaw("Mouse ScrollWheel"));
        hotbar = (hotbar - 1 + scroll) % 2 + 1; // TODO update me 

        fire = Input.GetButton("Fire");
        
        // This should go after we get inputs
        base.UpdateInputs();
    }

    protected override void SwitchedWeapons() {
        base.SwitchedWeapons();
        
        // Color hotbar slots
        for (int i = 0; i < hotbarImages.Length; i++) {
            Color color = i + 1 == hotbar ? Color.cyan : Color.white;
            hotbarImages[i].color = color;
        }
        
        audio.clip = clip_switchWeapons;
        audio.pitch = 1.05f - Random.Range(0f, 0.1f); // a bit of noise
        audio.Play();
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