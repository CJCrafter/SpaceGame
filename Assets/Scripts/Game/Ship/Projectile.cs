using System.Collections;
using UnityEngine;

public class Projectile : ForceEntity {

    public float thrust = 1f;
    public float rotation = 0f;
    public float damage = 40f;
    public float push = 100f;
    public float radius;

    [Header("Explosion Trigger")] 
    public float timeAlive = 15f;
    public bool triggerOnTime = true;
    public GameObject target;
    
    [Header("Visuals")]
    public GameObject impactParticlePrefab;
    public GameObject projectileParticlePrefab;
    public GameObject muzzleParticlePrefab;
    public GameObject[] trailParticlePrefabs;
    [HideInInspector]
    public Vector3 impactNormal;

    private GameObject impactParticle;
    private GameObject projectileParticle;
    private GameObject muzzleParticle;
    private GameObject[] trailParticles;
    
    private bool alreadyCollided;

    protected override void Start() {
        base.Start();
        StartCoroutine(KillOnTime());
        
        Transform transform = this.transform;
        projectileParticle = Instantiate(projectileParticlePrefab, transform.position, transform.rotation);
        projectileParticle.transform.parent = transform;
        if (muzzleParticlePrefab) {
            muzzleParticle = Instantiate(muzzleParticlePrefab, transform.position, transform.rotation);
            muzzleParticle.transform.rotation *= Quaternion.Euler(180, 0, 0);
            Destroy(muzzleParticle, 1.5f);
        }
    }

    public void OnCollisionEnter(Collision collision) {
        if (alreadyCollided)
            return;
        
        Transform transform = this.transform;
        impactParticle = Instantiate(impactParticlePrefab, transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal));
        foreach (GameObject trailPrefab in trailParticlePrefabs) {
            GameObject trail = transform.Find(projectileParticlePrefab.name + "/" + trailPrefab.name).gameObject;
            trail.transform.parent = null;
            Destroy(trail, 3f);
        }
        
        Destroy(projectileParticle, 3f);
        Destroy(impactParticle, 5f);
        Destroy(gameObject);

        ParticleSystem[] trails = transform.GetComponentsInChildren<ParticleSystem>();
        for (int i = 1; i < trails.Length; i++) {
            ParticleSystem trail = trails[i];

            if (trail.gameObject.name.Contains("Trail")) {
                trail.transform.SetParent(null);
                Destroy(trail.gameObject);
            }
        }
        
        Collide();
    }

    public override Vector3 CalculateThrust() {
        Vector3 rotation = transform.forward;
        if (target != null && this.rotation != 0.0f)
            Vector3.RotateTowards(rotation, target.transform.position - transform.position, this.rotation, 0);

        return rotation * thrust;
    }

    public void Collide() {
        alreadyCollided = true;

        Vector3 origin = transform.position;
        foreach (ForceEntity entity in universe.all) {
            float distance = MathUtil.Distance(entity.transform.position, origin);
            float factor = -damage / (radius * radius) * distance * distance + damage;

            if (!(factor > 0f)) 
                continue;
            
            entity.body.AddExplosionForce(push, origin, radius); // radius should be 0 for instant
            if (entity is Ship ship)
                ship.ApplyDamage(this, damage * factor);
        }
        
        Destroy(this);
    }

    private IEnumerator KillOnTime() {
        yield return new WaitForSeconds(timeAlive);

        switch (triggerOnTime) {
            case true when !alreadyCollided:
                Collide();
                break;
            case false:
                Destroy(this);
                break;
        }
    } 
}