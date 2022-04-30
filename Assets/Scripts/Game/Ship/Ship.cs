using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Ship : ForceEntity {

    public const int HOTBAR_LASER = 1;
    public const int HOTBAR_ROCKET = 2;
    
    [UnityEngine.Range(1, 9)] public int hotbar = 1;
    [Min(0)] public float engineAcceleration = 35f;
    [Min(0)] public float sensitivity = 0.75f;
    
    // Sounds effect stuff and visual effects
    [HideInInspector] public AudioSource audio;
    
    // Ships have different weapons
    protected Shield shield;
    protected Engine[] rockets;
    protected Phaser[] phasers;

    
    protected override void Start() {
        base.Start();

        audio = GetComponent<AudioSource>();
        shield = transform.GetComponentInChildren<Shield>();
        rockets = transform.GetComponentsInChildren<Engine>();
        phasers = transform.GetComponentsInChildren<Phaser>();
    }
    
    public void FireWeapon() {
        foreach (Phaser phaser in phasers) {
            phaser.RemoveBeam();
            phaser.SpawnBeam();
        }
    }

    public void StopFireWeapon() {
        foreach (Phaser phaser in phasers) {
            phaser.RemoveBeam();
        }
    }
}