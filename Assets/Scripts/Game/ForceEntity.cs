using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ForceEntity : MonoBehaviour {

    public float dragCoefficient = 0.75f;
    public float mass = 136078f; // mass of fully loaded starship
    public bool debugForces;

    public Vector3 localUp => (transform.position - strongestGravity.transform.position).normalized;

    [SerializeField, HideInInspector] private float[,,] crossSectionLookup;
    [SerializeField, HideInInspector] private Mesh mesh;
    protected Universe universe;
    protected GravityObject strongestGravity;

    protected Rigidbody body;
    protected float volume;
    
    protected virtual void Start() {
        universe = FindObjectOfType<Universe>();
        body = GetComponent<Rigidbody>();
        if (FindObjectOfType<MeshFilter>() != null) {
            Mesh shared = FindObjectOfType<MeshFilter>().sharedMesh;

            if (mesh != shared) {
                crossSectionLookup = null;
                mesh = shared;
            }
            
            volume = MeshUtil.CalculateVolume(mesh);
            Debug.Log("Calculated " + volume + " for volume");
        }

        
        float furthestPoint = mesh.bounds.extents.magnitude;
        crossSectionLookup ??= new float[3, 3, 3];
        
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
                    float step = furthestPoint / detail;
                    for (int i = -detail; i < detail; i++) {
                        for (int j = -detail; j < detail; j++) {

                            // 'o' is short for origin. 
                            Vector3 o = transform.position + a * i * step + b * j * step - direction * furthestPoint;
                            Physics.Raycast(new Ray(o, direction), out RaycastHit hit, furthestPoint * 2f);
                            bool collides = hit.collider != null && hit.collider.gameObject == gameObject;
                            //if (x == 1 && y == 1 && z == 1)
                            //    Debug.DrawRay(o, direction * furthestPoint * 2f, collides ? Color.green : Color.red, 100);
                            
                            if (!collides)
                                continue;

                            total++;
                        }
                    }
                    
                    // This multiplier should be proportional to 'furthestPoint'.
                    // It helps us to determine the conversion rate between pixels
                    // and meters. 
                    float multiplier = furthestPoint * 10f;
                    crossSectionLookup[x + 1, y + 1, z + 1] = (float) total / (detail * detail) * multiplier;
                    Debug.Log("Calculated " + crossSectionLookup[x + 1, y + 1, z + 1] + " for " + direction);
                }   
            }
        }
    }
    
    private void Update() {
        UpdateInputs();
        CalculateForces();
        
        transform.position += body.velocity * Time.deltaTime;
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
            Debug.DrawRay(origin, drag * 1000, Color.red);
            Debug.DrawRay(origin, buoyancy * 1000, Color.blue);
            Debug.DrawRay(origin, thrust * 1000, Color.yellow);
            foreach (Vector3 ray in gravity) 
                Debug.DrawRay(origin, ray * 1000, Color.green);
        }
        
        body.velocity += drag * Time.deltaTime;
        //body.velocity += buoyancy * Time.deltaTime;
        body.velocity += thrust * Time.deltaTime;
        foreach (Vector3 ray in gravity)
            body.velocity += ray * Time.deltaTime;
    }

    /**
     * Drag is always opposite to motion. Since drag is dependent on the area
     * exposed to the fluid (NOT VOLUME), we can do the "belly flop maneuver"
     * (flip sideways to increase drag). TECHNICALLY the drag coefficient will
     * also change based on direction, but let's not go overboard with the maths.
     *
     * Equation: F = 0.5 * fAv^2
     * f = drag coefficient, 0.3 = car, 0.05 = plane foil, 1.0 = horizontal 
     */
    public Vector3 CalculateDrag() {
        Vector3 velocity = body.velocity;
        Vector3 force = new Vector3(velocity.x * velocity.x, velocity.y * velocity.y, velocity.z * velocity.z);

        Vector3 n = force.normalized;
        float area = crossSectionLookup[Mathf.RoundToInt(n.x) + 1, Mathf.RoundToInt(n.y) + 1, Mathf.RoundToInt(n.z) + 1];
        force *= 0.5f * dragCoefficient * area * Time.deltaTime;
        
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
        float density = 0.01f; // temporary!! fix me

        return localUp * density * volume * Time.deltaTime;
    }

    public Vector3[] CalculateGravity() {
        strongestGravity = null;
        float strongestForce = 0f;
        var temp = new Vector3[universe.hasGravity.Count];
        
        for (var i = 0; i < universe.hasGravity.Count; i++) {
            GravityObject obj = universe.hasGravity[i];

            float force = obj.GetAccelerationAt(transform.position, out Vector3 vector);
            
            // Save the strongest source of gravity to determine which
            // direction is up for the player. 
            if (force >= strongestForce) {
                strongestGravity = obj;
                strongestForce = force;
            }
            
            temp[i] = vector;
        }

        return temp;
    }

    public virtual Vector3 CalculateThrust() {
        return Vector3.zero;
    }
}