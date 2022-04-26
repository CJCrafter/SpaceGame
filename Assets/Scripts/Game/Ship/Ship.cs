using UnityEngine;

public class Ship : ForceEntity {

    [Range(1, 9)] public int hotbar = 1;
    [Min(0)] public float engineAcceleration = 35f;
    [Min(0)] public float sensitivity = 0.75f;
    
    protected Rocket[] rockets;

    // Ships may have many different kinds of weapons
    private SciFiBeamStatic[] beams;
    
    protected override void Start() {
        base.Start();

        rockets = transform.GetComponentsInChildren<Rocket>();
    }
    
    public void FireWeapon() {
        
    }
}