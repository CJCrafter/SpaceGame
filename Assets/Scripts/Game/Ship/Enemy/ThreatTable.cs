using System.Collections.Generic;
using System.Linq;
using Game;
using UnityEngine;

public class ThreatTable : MonoBehaviour {
    
    public float assessmentInterval;
    [Range(0f, 2f)] public float healthWeight;
    [Range(0f, 2f)] public float damageWeight;
    [Range(0f, 2f)] public float distanceWeight;
    [Min(0f)] public float trackingRange = 5000f;
    [Min(0f)] public float trackingRangeNoise = 100f;
    [Min(0f)] public float trackingTime = 14f;
    [Min(0f)] public float trackingTimeNoise = 1;
    
    private Ship ship;
    private Vector3[] eyes;
    private List<ThreatData> _threats;
    private float lastAssessment;
    private float lastSearch;
    private ThreatData _threat;

    public void Init() {
        if (ship != null)
            return;
        
        ship = GetComponent<Ship>();

        var eyes = (from Transform child in ship.transform where child.gameObject.name.StartsWith("Eye") select child.gameObject).ToList();
        this.eyes = new Vector3[eyes.Count];
        for (var i = 0; i < eyes.Count; i++) {
            GameObject eye = eyes[i];
            this.eyes[i] = eye.transform.position - ship.transform.position;
            Destroy(eye);
        }

        Events.ENTITY_SPAWN.AddListener(OnAddEntity);
        Events.ENTITY_DESTROY.AddListener(OnRemoveEntity);
        Events.ENTITY_DAMAGE.AddListener(OnDamageEntity);
    }

    public void OnAddEntity(Events.EntitySpawnEvent e) {
        if (ship != e.entity)
            threats.Add(new ThreatData(this, e.entity));
    }

    public void OnRemoveEntity(Events.EntityDestroyEvent e) {
        threats.RemoveAll(element => element.entity == e.entity);
    }

    public void OnDamageEntity(Events.EntityDamageEvent e) {
        if (ship != e.damaged || ship == e.damager)
            return;

        threats.Find(element => element.entity == e.damager).damageReceived += e.totalDamage;
    }

    public bool HasTarget() {
        ThreatData target = threat;
        if (target == null)
            return false;

        float time = target.TimeSinceSeen();
        float confidence = 1f;
        confidence -= time / (trackingTime / 2f);
        confidence -= target.distance / trackingRange;
        return confidence > -1f;
    }
    
    public void Update() {
        if (Time.timeSinceLevelLoad > lastSearch + 3.0f) {
            foreach (ThreatData data in threats) {
                data.Update();
            }

            lastSearch = Time.timeSinceLevelLoad;
        }
    }
    
    public ThreatData threat {
        get {
            if (_threat != null && !(Time.timeSinceLevelLoad > lastAssessment + assessmentInterval)) 
                return _threat;

            Init();
            lastAssessment = Time.timeSinceLevelLoad;

            bool changed = false;
            float max = _threat?.Evaluate() ?? float.MinValue;
            foreach (ThreatData option in threats) {
                if (option == _threat)
                    continue;
                
                float threatLevel = option.Evaluate();
                if (threatLevel >= max) {
                    changed = true;
                    max = threatLevel;
                    _threat = option;
                }
            }
            
            if (changed) 
                _threat?.Update();
            
            // Make sure to update our information about that threat
            return _threat;
        }
    }

    public List<ThreatData> threats {
        get {
            if (_threats != null && _threats.Count != 0)
                return _threats;

            _threats ??= new List<ThreatData>();
            foreach (Ship ship in FindObjectsOfType<Ship>()) {
                if (this.ship != ship)
                    _threats.Add(new ThreatData(this, ship));
            }

            return _threats;
        }
    }

    public ShootData GetTargetLocation(float inTime = 0f) {
        ThreatData target = threat;
        if (target == null)
            return null;

        float time = target.TimeSinceSeen() + inTime;

        // This is the position of the ship assuming there have been no forces.
        // This, of course, is a bogus assumption. Perhaps we should consider 
        // taking "ForceEntity" equations into effect, or at least GravityObject.
        Vector3 expectedPosition = target.lastKnownPosition + target.lastKnownVelocity * time;
        
        // Now lets calculate the "confidence" of our shot. We start with a
        // confidence of 1 and strip that down to some negative number. 
        float confidence = 1f;
        confidence -= time / (trackingTime / 2f) * trackingTimeNoise;
        confidence -= target.distance / trackingRange * trackingRangeNoise;

        Vector3 noise = Random.onUnitSphere * (1f - confidence);

        return new ShootData
            {
                position = expectedPosition,
                noise = noise,
                confidence = confidence
            };
    }
    
    
    public class ThreatData {

        private readonly ThreatTable root;
        public readonly ForceEntity entity;
        public Vector3 lastKnownPosition;
        public Vector3 lastKnownVelocity;
        public float lastKnownHealth;
        public float damageReceived;
        public float distance;
        public float lastSeenTime;

        internal ThreatData(ThreatTable root, ForceEntity entity) {
            this.root = root;
            this.entity = entity;
            
            Update();
        }

        internal void Update() {
            Debug.Log("Updating " + entity.gameObject);

            foreach (Vector3 eye in root.eyes) {
                Vector3 origin = root.transform.position + eye;
                Ray cast = new Ray(origin, entity.transform.position - origin);
                const int layerFilter = 1 << 7;
                bool hit = Physics.Raycast(cast, out RaycastHit ray, 2500f, layerFilter);

                if (hit && ray.transform.gameObject.CompareTag("Entity") && ray.transform.GetComponentInParent<ForceEntity>() == entity) {
                    lastSeenTime = Time.timeSinceLevelLoad;
                    lastKnownPosition = entity.transform.position;
                    lastKnownVelocity = entity.body.velocity;
                    lastKnownHealth = entity.health;
                    distance = ray.distance;
                    break;
                }
            }
        }

        internal float Evaluate() {
            
            // Ships should be able to target:
            //   1. weak ships
            //   2. ships that dealt a lot of damage
            //   3. close ships
            //   4. ships that are aiming at us
            float healthFactor = 1f - lastKnownHealth / entity.maxHealth;
            float damageFactor = damageReceived / root.ship.maxHealth;
            float distanceFactor = (1000f - distance) / distance;

            return healthFactor * root.healthWeight + damageFactor * root.damageWeight +
                   distanceFactor * root.distanceWeight;
        }

        internal float TimeSinceSeen() {
            return Time.timeSinceLevelLoad - lastSeenTime;
        }
    }

    public class ShootData {
        public Vector3 position;
        public Vector3 noise;
        public float confidence;

        public (Vector3 origin, Vector3 direction) Ray(Vector3 origin) {
            Vector3 direction = position - origin + noise;

            return (origin, direction);
        }
    }
}