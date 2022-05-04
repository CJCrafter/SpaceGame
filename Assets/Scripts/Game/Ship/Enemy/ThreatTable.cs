using System.Collections.Generic;
using Game;
using UnityEngine;

public class ThreatTable : MonoBehaviour {
    
    public float assessmentInterval;
    [Range(0f, 2f)] public float healthWeight;
    [Range(0f, 2f)] public float damageWeight;
    [Range(0f, 2f)] public float distanceWeight;
    
    private Ship ship;
    private List<ThreatData> _threats;
    private float lastAssessment;
    private float lastSearch;
    private ThreatData _threat;

    public void Init() {
        if (ship != null)
            return;
        
        ship = GetComponent<Ship>();

        Events.SHIP_SPAWN.AddListener(OnAddShip);
        Events.SHIP_DESTROY.AddListener(OnRemoveShip);
        Events.SHIP_DAMAGE.AddListener(OnDamageShip);
    }

    public void OnAddShip(Events.ShipSpawnEvent e) {
        Debug.Log("ADDED");
        if (ship != e.ship)
            threats.Add(new ThreatData(this, e.ship));
    }

    public void OnRemoveShip(Events.ShipDestroyEvent e) {
        threats.RemoveAll(element => element.ship == e.ship);
    }

    public void OnDamageShip(Events.ShipDamageEvent e) {
        if (ship != e.damaged || ship == e.damager)
            return;

        threats.Find(element => element.ship == e.damager).damageReceived += e.totalDamage;
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

            float max = float.MinValue;
            _threat = null;
            foreach (var threat in threats) {
                float threatLevel = threat.Evaluate();
                if (threatLevel >= max) {
                    max = threatLevel;
                    _threat = threat;
                }
            }

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

    public ShootData GetTargetLocation() {
        ThreatData target = threat;
        if (target == null)
            return null;

        // TODO Add support to track future position
        float time = target.TimeSinceSeen();

        // This is the position of the ship assuming there have been no forces.
        // This, of course, is a bogus assumption. Perhaps we should consider 
        // taking "ForceEntity" equations into effect, or at least GravityObject.
        Vector3 expectedPosition = target.lastKnownPosition + target.lastKnownVelocity * time;
        
        // Now lets calculate the "confidence" of our shot. We start with a
        // confidence of 1 and strip that down to some negative number. 
        float confidence = 1f;
        confidence -= time / 7f;
        confidence -= target.distance / 60f;

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
        public readonly Ship ship;
        public Vector3 lastKnownPosition;
        public Vector3 lastKnownVelocity;
        public float lastKnownHealth;
        public float damageReceived;
        public float distance;
        public float lastSeenTime;

        internal ThreatData(ThreatTable root, Ship ship) {
            this.root = root;
            this.ship = ship;
            
            Update();
        }

        internal void Update() {

            bool hit = Physics.Raycast(new Ray(root.transform.position, ship.transform.position - root.transform.position), out RaycastHit ray);
            if (hit && ray.transform.gameObject == ship.gameObject && (ship.cloak == null || !ship.cloak.active)) {
                lastSeenTime = Time.timeSinceLevelLoad;
                lastKnownPosition = ship.transform.position;
                lastKnownVelocity = ship.body.velocity;
                lastKnownHealth = ship.health;
                distance = ray.distance;
            }
        }

        internal float Evaluate() {
            
            // Ships should be able to target:
            //   1. weak ships
            //   2. ships that dealt a lot of damage
            //   3. close ships
            //   4. ships that are aiming at us
            float healthFactor = 1f - lastKnownHealth / ship.maxHealth;
            float damageFactor = damageReceived / root.ship.maxHealth;
            float distanceFactor = (1000f - distance) / distance;

            return healthFactor * root.healthWeight + damageFactor * root.damageWeight +
                   distanceFactor * root.distanceWeight;
        }

        internal float TimeSinceSeen() {
            return Time.timeSinceLevelLoad - lastSeenTime;
        }
        
        public override int GetHashCode() {
            return ship.gameObject.GetHashCode();
        }
    }

    public class ShootData {
        public Vector3 position;
        public Vector3 noise;
        public float confidence;

        public (Vector3 origin, Vector3 direction) Ray(Vector3 origin) {
            Vector3 direction = (position - origin + noise);

            return (origin, direction);
        }
    }
}