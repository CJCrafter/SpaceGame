using Game;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Ship : ForceEntity {

    public const int HOTBAR_LASER = 1;
    public const int HOTBAR_MISSILE = 2;
    
    [Min(0f)] public float engineAcceleration = 35f;
    [Min(0f)] public float sensitivity = 0.75f;
    [Min(0f)] public float laserDPS = 20f;
    
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
        
        hotbar = 1;
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

    public virtual void ApplyDamage(ForceEntity source, float damage, bool impulse = true) {
        float beforeAdjustments = damage;

        if (shield != null) {
            bool front = Vector3.Dot(transform.forward, transform.position - source.transform.position) >= 0f;
            damage = shield.ApplyDamage(front, damage);
        }

        health -= damage;
        Events.ENTITY_DAMAGE.Invoke(new Events.EntityDamageEvent
        {
            damager = source,
            damaged = this,
            totalDamage = beforeAdjustments,
            appliedDamage = damage
        });

        if (health <= 0f) {
            Destroy(gameObject);
            Events.ENTITY_DESTROY.Invoke(new Events.EntityDestroyEvent(this));
        }
    }
    
    public void FireWeapon() {
        if (hotbar == HOTBAR_LASER) {
            if (!wasFiring) {
                foreach (Phaser phaser in phasers) {
                    phaser.RemoveBeam();
                    phaser.SpawnBeam();
                }
            }

            foreach (Phaser phaser in phasers) {
                if (phaser.hit.transform == null)
                    continue;
                
                Ship ship = phaser.hit.transform.GetComponentInParent<Ship>();
                if (ship != null)
                    ship.ApplyDamage(this, laserDPS * Time.fixedDeltaTime);
            }
        }

        else if (hotbar != HOTBAR_LASER) {
            foreach (Launcher launcher in launchers) {
                launcher.Fire();
            }
        }
    }

    public void StopFireWeapon() {
        foreach (Phaser phaser in phasers) {
            phaser.RemoveBeam();
        }
    }

    public virtual Quaternion GetShootDirection(Vector3 origin) {
        return transform.rotation;
    }

    public virtual ForceEntity GetTarget() {
        return null;
    }
}