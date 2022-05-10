using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(ShowOrbit))]
public class ForceEntity : MonoBehaviour {

    public float dragCoefficient = 0.75f;
    public float mass = 136078f; // mass of fully loaded starship
    public bool hasGravity;
    public bool debugForces;
    public Vector3 initialVelocity;
    public float health = 100f;
    public float maxHealth { get; private set; }

    public ForceEntity strongestGravity { get; private set; }
    public Vector3 localUp => (transform.position - strongestGravity.transform.position).normalized;

    protected Universe universe;
    [SerializeReference, HideInInspector] public MeshData mesh;
    [HideInInspector] public Rigidbody body;

    protected virtual void Start() {
        universe = FindObjectOfType<Universe>();
        body = GetComponent<Rigidbody>();
        body.mass = mass;
        maxHealth = health;
        body.useGravity = false;
        body.AddForce(initialVelocity, ForceMode.VelocityChange);
        
        if (mesh == null && GetComponent<MeshFilter>())
            mesh = new MeshData(this);
        mesh?.Init();
        SetTagRecursive(gameObject);
    }

    private static void SetTagRecursive(GameObject obj) {
        obj.tag = "Entity";
        obj.layer = 7; // Entity layer
        foreach (Transform child in obj.transform)
            SetTagRecursive(child.gameObject);
    }

    private void FixedUpdate() {
        if (!Application.isPlaying)
            return;
        
        UpdateInputs();
        CalculateForces();
    }

    protected virtual void OnValidate() {
        GetComponent<Rigidbody>().mass = mass;
    }

    protected virtual void UpdateInputs() {
    }

    public virtual void CalculateForces() {
        Vector3 drag = CalculateDrag();
        Vector3[] gravity = CalculateGravity(); // BEFORE BUOYANCY!
        Vector3 buoyancy = CalculateBuoyancy();
        Vector3 thrust = CalculateThrust();

        if (debugForces) {
            Vector3 origin = transform.position;
            Debug.DrawRay(origin, drag, Color.red);
            Debug.DrawRay(origin, buoyancy, Color.blue);
            Debug.DrawRay(origin, thrust, Color.yellow);
            foreach (Vector3 ray in gravity) 
                Debug.DrawRay(origin, ray, Color.green);
        }
        
        body.AddForce(drag, ForceMode.Impulse);
        body.AddForce(buoyancy, ForceMode.Impulse);
        body.AddForce(thrust, ForceMode.Impulse);
        foreach (Vector3 ray in gravity)
            body.AddForce(ray, ForceMode.Impulse);
    }

    /**
     * Drag is always opposite to motion. Since drag is dependent on the area
     * exposed to the fluid (NOT VOLUME), we can do the "belly flop maneuver"
     * (flip sideways to increase drag). TECHNICALLY the drag coefficient will
     * also change based on direction, but let's not go overboard with the maths.
     *
     * Equation: F = 0.5 * pfAv^2
     * p = fluid density
     * f = drag coefficient, 0.3 = car, 0.05 = plane foil, 1.0 = horizontal
     * A = Cross area exposed to fluid (based on velocity)
     * v = velocity
     */
    public Vector3 CalculateDrag() {
        if (mesh == null)
            return Vector3.zero;
        
        Vector3 velocity = body.velocity;
        Vector3 force = new Vector3(velocity.x * velocity.x, velocity.y * velocity.y, velocity.z * velocity.z);

        float area = mesh.Area(force.normalized);
        force *= 0.5f * CalculateDensity() * dragCoefficient * area * Time.fixedDeltaTime;
        
        // Make sure the drag is opposite to motion
        force.x *= velocity.x > 0 ? -1 : 1;
        force.y *= velocity.y > 0 ? -1 : 1;
        force.z *= velocity.z > 0 ? -1 : 1;

        return force;
    }

    /**
     * Buoyancy is always opposite to gravity. To simplify calculations, we
     * will only consider the current strongest gravitational force for
     * buoyancy. We must consider that the density of the atmosphere is lower
     * at high elevations, and greater at low elevations. Derived from "the
     * buoyant force on an object equals the weight of the fluid it displaces"
     * 
     * Equation: F = -pgV
     * p = density of the fluid
     * g = acceleration due to gravity
     * V = displaced fluid volume (volume of spaceship)
     */
    public Vector3 CalculateBuoyancy() {
        if (mesh == null)
            return Vector3.zero;
        
        Vector3 between = strongestGravity.transform.position - transform.position;
        float distanceSquared = between.sqrMagnitude;
        float g = Universe.gravitationalConstant * strongestGravity.mass / distanceSquared;
        return localUp * CalculateDensity() * g * mesh.volume * Time.fixedDeltaTime;
    }

    public float CalculateDensity() {
        if (strongestGravity == null)
            return 0f;
        
        // Currently only supports planets
        Planet planet = strongestGravity.GetComponent<Planet>();
        if (planet == null)
            return 0f;

        float outer = planet.radius * planet.atmosphere.atmospherePercentage;
        float distance = MathUtil.Distance(transform.position, strongestGravity.transform.position);
        if (distance > outer)
            return 0f;
        if (distance < planet.elevationBounds.max * planet.radius * planet.biomes.oceanHeight)
            return 997f; // density of water

        return MathUtil.Remap(distance, 0, outer, 0, planet.atmosphere.atmosphereDensity);
    }

    public Vector3[] CalculateGravity() {
        strongestGravity = null;
        float strongestForce = 0f;
        var temp = new Vector3[universe.hasGravity.Count];
        
        for (var i = 0; i < universe.hasGravity.Count; i++) {
            ForceEntity obj = universe.hasGravity[i];
            if (this == obj)
                continue;

            Vector3 between = obj.transform.position - transform.position;
            float distanceSquared = between.sqrMagnitude;
            float force = Universe.gravitationalConstant * obj.mass / distanceSquared;
            
            between.Normalize();
            between *= force * Time.fixedDeltaTime;


            // Save the strongest source of gravity to determine which
            // direction is up for the player. 
            if (force >= strongestForce) {
                strongestGravity = obj;
                strongestForce = force;
            }

            temp[i] = between * mass;
        }

        return temp;
    }

    public virtual Vector3 CalculateThrust() {
        return Vector3.zero;
    }

    [Serializable]
    public class MeshData {

        [SerializeField] private ForceEntity root;
        [SerializeField] private string meshName;
        [SerializeField] private bool calculated;
        [SerializeField] public float volume;
        [SerializeField] public float radius;
        [SerializeField] public float[] areas; // 3x3x3 box array, unwrapped
        
        public MeshData(ForceEntity root) {
            Debug.Log("First time!");
            Mesh mesh = root.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh == null)
                throw new Exception(root.name + " missing mesh");

            this.root = root;
            this.meshName = mesh.name;
            this.areas = new float[3 * 3 * 3];
        }

        public void Init() {
            Mesh mesh = root.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh == null)
                throw new Exception(root.name + " missing mesh");

            // Make sure to skip calculations, so long as they are up to date.
            // If the name of the mesh changes, we know we need to recalculate.
            if (calculated && meshName == mesh.name)
                return;

            float startTime = Time.realtimeSinceStartup;
            radius = mesh.bounds.extents.magnitude;
            volume = MeshUtil.CalculateVolume(mesh);

            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    for (int z = -1; z <= 1; z++) {
                        if (x == 0 && y == 0 && z == 0)
                            continue;

                        // Form an axis using 3 perpendicular vectors
                        Vector3 direction = -new Vector3(x, y, z).normalized;
                        Vector3 a = Vector3.Cross(direction, new Vector3(-2, 7, -13).normalized).normalized;
                        Vector3 b = Vector3.Cross(direction, a).normalized;

                        // Loop through 'horizontally' and 'vertically' and ray
                        // trace. Total up all positive hits to estimate the area.
                        int total = 0;
                        const int detail = 32;
                        float step = radius / detail;
                        for (int i = -detail; i < detail; i++) {
                            for (int j = -detail; j < detail; j++) {

                                // 'o' is short for origin. 
                                Vector3 o = root.transform.position + a * i * step + b * j * step -
                                            direction * radius;
                                Physics.Raycast(new Ray(o, direction), out RaycastHit hit, radius * 2f);
                                bool collides = hit.collider != null && hit.collider.gameObject == root.gameObject;
                                //if (x == 1 && y == 1 && z == 1)
                                //    Debug.DrawRay(o, direction * furthestPoint * 2f, collides ? Color.green : Color.red, 100);

                                if (!collides)
                                    continue;

                                total++;
                            }
                        }
                        
                        // We need to multiply by the size of each pixel (Which should
                        // be 'step', but this requires verification).
                        float multiplier = step * step;
                        areas[(z + 1) * 9 + (y + 1) * 3 + (x + 1)] = total * multiplier;
                        //Debug.Log("Calculated " + crossSectionLookup[x + 1, y + 1, z + 1] + " for " + direction);
                    }
                }
            }

            calculated = true;
            Debug.Log("Took " + (Time.realtimeSinceStartup - startTime) + "s to calculate " + mesh.name + " properties.");
            Debug.Log("  Volume: " + volume);
            Debug.Log("  Areas: " + string.Join(", ", areas));
        }
        
        public float Area(Vector3 direction) {
            int x1 = Mathf.FloorToInt(direction.x);
            int x2 = Mathf.CeilToInt(direction.x);
            int y1 = Mathf.FloorToInt(direction.y);
            int y2 = Mathf.CeilToInt(direction.y);
            int z1 = Mathf.FloorToInt(direction.z);
            int z2 = Mathf.CeilToInt(direction.z);
            
            // Now we need to check each the 4 points involved and determine
            // their weights.
            float total = 0f;
            for (int x = x1; x < x2; x++) {
                for (int y = y1; y < y2; y++) {
                    for (int z = z1; z < z2; z++) {
                        if (x == 0 && y == 0 && z == 0)
                            continue;

                        float distance = MathUtil.Distance(x - direction.x, y - direction.y, z - direction.z);
                        float weight = 1 - distance;
                        total += areas[(z + 1) * 9 + (y + 1) * 3 + (x + 1)] * weight;
                    }
                }    
            }

            return total;
        }
    }
}