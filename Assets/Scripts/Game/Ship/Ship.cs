using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Ship : ForceEntity {

    public const int HOTBAR_LASER = 1;
    public const int HOTBAR_MISSILE = 2;
    
    [Min(0f)] public float engineAcceleration = 35f;
    [Min(0f)] public float sensitivity = 0.75f;
    [Min(0f)] public float health;

    
    [HideInInspector] public GameObject target;
    [HideInInspector] public float maxHealth;
    private bool wasFiring;
    protected bool fire;
    private int _hotbar;
    public int hotbar {
        get => _hotbar;
        protected set {
            bool switched = _hotbar != value;
            _hotbar = value;
            if (switched) 
                SwitchedWeapons();
        }
    }

    // Sounds effect stuff and visual effects
    [HideInInspector] public AudioSource audio;
    
    // Ships have different weapons
    public Shield shield { get; private set; }
    public Cloak cloak { get; private set; }
    public Engine[] rockets { get; private set; }
    public Phaser[] phasers { get; private set; }
    public Launcher[] launchers { get; private set; } 

    
    protected override void Start() {
        base.Start();

        audio = GetComponent<AudioSource>();
        shield = transform.GetComponentInChildren<Shield>();
        cloak = transform.GetComponentInChildren<Cloak>();
        rockets = transform.GetComponentsInChildren<Engine>();
        phasers = transform.GetComponentsInChildren<Phaser>();
        launchers = transform.GetComponentsInChildren<Launcher>();

        _hotbar = 1;
    }

    protected virtual void SwitchedWeapons() {
        StopFireWeapon();
    }

    protected override void UpdateInputs() {
        base.UpdateInputs();
        
        if (!fire && wasFiring)
            StopFireWeapon();

        if (fire)
            FireWeapon();
        
        wasFiring = fire;
    }
    
    public void FireWeapon() {
        switch (hotbar) {

            case HOTBAR_LASER: {
                if (wasFiring)
                    break;
                
                foreach (Phaser phaser in phasers) {
                    phaser.RemoveBeam();
                    phaser.SpawnBeam();
                }
                
                break;
            }

            case HOTBAR_MISSILE: {
                foreach (Launcher launcher in launchers) {
                    launcher.Fire();
                }

                break;
            }
        }
    }

    public void StopFireWeapon() {
        foreach (Phaser phaser in phasers) {
            phaser.RemoveBeam();
        }
    }
}