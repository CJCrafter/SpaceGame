using System.Collections.Generic;
using Game;
using UnityEngine;

public class ThreatTable : MonoBehaviour {
    
    public float assessmentInterval;
    [Range(0f, 2f)] public float healthWeight;
    [Range(0f, 2f)] public float damageWeight;
    [Range(0f, 2f)] public float distanceWeight;
    
    private Ship ship;
    private Dictionary<Ship, ThreatData> possibleThreats;
    private float lastAssessment;
    private float lastSearch;
    private ThreatData _threat;
    public ThreatData threat {
        get {
            if (_threat == null || Time.timeSinceLevelLoad > lastAssessment + assessmentInterval) {
                lastAssessment = Time.timeSinceLevelLoad;

                float max = float.MinValue;
                _threat = null;
                foreach (var threat in possibleThreats.Values) {
                    float threatLevel = threat.Evaluate();
                    if (threatLevel >= max) {
                        max = threatLevel;
                        _threat = threat;
                    }
                }
            }

            return _threat;
        }
    }


    public void Start() {
        ship = GetComponent<Ship>();
        possibleThreats = new Dictionary<Ship, ThreatData>();
        foreach (Ship ship in FindObjectsOfType<Ship>()) {
            possibleThreats[ship] = new ThreatData(this, ship);
        }
        
        Events.SHIP_SPAWN.AddListener(OnAddShip);
        Events.SHIP_DESTROY.AddListener(OnRemoveShip);
        Events.SHIP_DAMAGE.AddListener(OnDamageShip);
    }

    public void OnAddShip(Events.ShipSpawnEvent e) {
        possibleThreats[e.ship] = new ThreatData(this, e.ship);
    }

    public void OnRemoveShip(Events.ShipDestroyEvent e) {
        possibleThreats[e.ship] = null;
    }

    public void OnDamageShip(Events.ShipDamageEvent e) {
        if (e.damaged != ship)
            return;

        possibleThreats[e.damaged].damageReceived += e.totalDamage;
    }
    
    public void Update() {
        if (Time.timeSinceLevelLoad > lastSearch + 3.0f) {
            foreach (ThreatData data in possibleThreats.Values) {
                data.Update();
            }
        }
    }

    public Vector3 GetTargetDirection() {
        ThreatData target = threat;

        // This is the position of the ship assuming there have been no forces.
        // This, of course, is a bogus assumption. Perhaps we should consider 
        // taking "ForceEntity" equations into effect, or at least GravityObject.
        Vector3 expectedPosition = target.lastKnownPosition + target.lastKnownVelocity * target.TimeSinceSeen();
        Vector3 actualPosition = target.ship.transform.position;
        RaycastHit ray;
        bool canSee = !ship.cloak.active && Physics.Raycast(new Ray(transform.position, actualPosition - transform.position), out ray);
        
        // Now lets calculate our "confidence" of our shot. A confidence [-1, 0]
        // means our information is shit, can we should basically shoot randomly
        // in the general direction. A confidence [0, 1] will lerp between the
        // expected and actual position
        
    }
    
    
    
    

    
    public class ThreatData {

        private readonly ThreatTable root;
        public readonly Ship ship;
        public Vector3 lastKnownPosition;
        public Vector3 lastKnownVelocity;
        public float lastKnownHealth;
        public float damageReceived;
        public float distanceSquared;
        public float lastSeenTime;

        internal ThreatData(ThreatTable root, Ship ship) {
            this.root = root;
            this.ship = ship;
        }

        internal void Update() {
            lastKnownPosition = ship.transform.position;
            lastKnownVelocity = ship.body.velocity;
            lastKnownHealth = ship.health;
            distanceSquared = MathUtil.SquareDistance(lastKnownPosition, root.ship.transform.position);
        }

        internal float Evaluate() {
            
            // Ships should be able to target:
            //   1. weak ships
            //   2. ships that dealt a lot of damage
            //   3. close ships
            //   4. ships that are aiming at us
            float healthFactor = 1f - lastKnownHealth / ship.maxHealth;
            float damageFactor = damageReceived / root.ship.maxHealth;
            float distanceFactor = (100_000_000f - distanceSquared) / distanceSquared;

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
        public Vector3 direction;
        public float confidence;
        public float speed;
    }
}