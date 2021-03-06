using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Launcher : MonoBehaviour {

    public ProjectileData missile;
    public AudioClip noAmmoClip;

    private Ship ship;
    private float lastShootTime;
    
    private void Start() {
        ship = transform.GetComponentInParent<Ship>(); 
    }

    public void Fire() {
        ProjectileData projectile = null;
        if (ship.hotbar == Ship.HOTBAR_MISSILE)
            projectile = missile;

        if (projectile == null)
            throw new Exception("Tried to fire " + ship.hotbar + " when that is invalid");

        // Handle cool down for the launcher + sounds
        if (Time.timeSinceLevelLoad < lastShootTime + projectile.cooldown) {
            ship.audio.clip = noAmmoClip;
            ship.audio.Play();
            return;
        }
        lastShootTime = Time.timeSinceLevelLoad;
        
        // Create the object and rotate it towards the target OR the ship's current rotation
        GameObject obj = Instantiate(projectile.prefab, transform.position, Quaternion.identity);
        obj.transform.rotation = ship.GetShootDirection(transform.position);
        obj.GetComponent<Projectile>().target = ship.GetTarget();

        Vector3 velocity = (ship.transform.forward + Random.onUnitSphere * projectile.spread) * projectile.speed;
        velocity += ship.GetComponent<Rigidbody>().velocity;
        obj.GetComponent<Rigidbody>().AddForce(velocity, ForceMode.VelocityChange);
    }

    [Serializable]
    public class ProjectileData {
        public GameObject prefab;
        public float speed;
        [Range(0f, 1f)] public float spread;
        [Min(0f)] public float cooldown = 0.5f;
    }
}