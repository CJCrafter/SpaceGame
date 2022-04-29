using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

[ExecuteInEditMode]
public class OrbitDebugger : MonoBehaviour {

    private class FakePlanet {
        internal string name;
        internal GameObject gameObject;
        internal Vector3 position;
        internal Vector3 velocity;
        internal float mass;
        internal bool hasGravity;
        internal bool usesGravity;
        internal Color color; 

        internal FakePlanet(GravityObject obj) {
            name = obj.name;
            gameObject = obj.gameObject;
            position = obj.transform.position;
            velocity = obj.velocity;
            mass = obj.mass;
            hasGravity = obj.hasGravity;
            usesGravity = obj.usesGravity;
            
            if (colorMap.ContainsKey(name))
                color = colorMap[name];
            else
                color = colorMap[name] = colors[Random.Range(0, colors.Length - 1)];
        }

        internal FakePlanet(ForceEntity obj) {
            name = obj.name;
            gameObject = obj.gameObject;
            position = obj.transform.position;
            velocity = obj.GetComponent<Rigidbody>().velocity;
            mass = obj.mass;
            hasGravity = false;
            usesGravity = true;
            
            if (colorMap.ContainsKey(name))
                color = colorMap[name];
            else
                color = colorMap[name] = colors[Random.Range(0, colors.Length - 1)];
        }

        internal void Update() {
            GravityObject temp = gameObject.GetComponent<GravityObject>();
            position = gameObject.transform.position;
            velocity = temp == null ? gameObject.GetComponent<Rigidbody>().velocity : temp.velocity;
            mass = temp == null ? gameObject.GetComponent<ForceEntity>().mass : temp.mass;
            if (temp != null) {
                hasGravity = temp.hasGravity;
                usesGravity = temp.usesGravity;
            }
        }
    }

    // Stores colors for each map
    private static SortedDictionary<string, Color> colorMap = new SortedDictionary<string, Color>();
    private static Color[] colors =
        {
            Color.blue, Color.cyan, Color.gray, Color.green, Color.magenta, Color.red, Color.yellow, 
            new Color(1f, 0.5f, 0f), new Color(0.5f, 0f, 1f)
        };

    [Min(1f)] public float ticksPerStep = 50f;
    [Min(1)] public int steps = 100;

    public bool showPlanets = true;
    public bool showEntities = true;
    public GravityObject relative;

    private Universe universe;
    private FakePlanet relativeCache;
    private List<FakePlanet> planets;
    private float lastUpdate;
    
    // This is just good information to display
    public List<string> planetNames;
    public List<string> entityNames;

    public void Init() {
        
        // Cache universe
        universe = FindObjectOfType<Universe>();
        if (universe == null)
            throw new MissingUniverseException();

        // Start by clearing out old planets
        planets = new List<FakePlanet>();
        planetNames = new List<string>();
        entityNames = new List<string>();
        
        // We need to wrap each planet it a "FakePlanet" so we
        // don't accidentally modify an existing planet.
        foreach (GravityObject obj in FindObjectsOfType<GravityObject>()) {
            FakePlanet fake = new FakePlanet(obj);
            planets.Add(fake);
            planetNames.Add(fake.name);
        }

        foreach (ForceEntity obj in FindObjectsOfType<ForceEntity>()) {
            FakePlanet fake = new FakePlanet(obj);
            planets.Add(fake);
            entityNames.Add(fake.name);
        }
    }

    private void OnValidate() {
        Init();
        relativeCache = planets.Where(planet => planet.gameObject == relative.gameObject).GetEnumerator().Current;
        ShowOrbits();
    }

    private void Update() {
        if (universe == null || Time.timeSinceLevelLoad > lastUpdate + 3f)
            ShowOrbits();
    }

    public void ShowOrbits() {
        if (!showEntities && !showPlanets)
            return;

        // First check if we need to initialize our lists. Afterwards we need to 
        // update the "fake" variables to make their positions and velocities are
        // true to where the planets currently are.
        if (universe == null || planets == null)
            Init();

        foreach (var planet in planets) {
            planet.Update();
        }

        // In order to draw our lines, we need to save the previous point for
        // each planet. This is updated for each step.
        Vector3[] start = new Vector3[planets.Count];
        for (int j = 0; j < planets.Count; j++) {
            start[j] = planets[j].position - GetRelativeVector();
        }

        // Simulate Gravity/Movement for each step, and add draw lines
        Vector3 initialReferenceFrame = GetRelativeVector();
        for (int i = 0; i < steps; i++) {
            Vector3 offset = GetRelativeVector() - initialReferenceFrame;
            
            SimulateGravity();
            SimulateMovement();

            for (var j = 0; j < planets.Count; j++) {
                Vector3 current = planets[j].position;
                current -= offset;
                Debug.DrawLine(start[j], current, planets[j].color);
                start[j] = current;
            }
        }
    }

    public Vector3 GetRelativeVector() {
        return relativeCache?.position ?? Vector3.zero;
    }

    private void SimulateGravity() {
        foreach (FakePlanet gravityEmitter in planets) {
            if (!gravityEmitter.hasGravity)
                continue;

            foreach (FakePlanet planet in planets) {
                if (!planet.usesGravity)
                    continue;
                
                Vector3 direction = gravityEmitter.position - planet.position;

                float distanceSquared = direction.sqrMagnitude;
                if (distanceSquared == 0.0f)
                    continue;

                float force = Universe.gravitationalConstant * gravityEmitter.mass / distanceSquared;

                direction.Normalize();
                direction *= force * Time.fixedDeltaTime * ticksPerStep;
                planet.velocity += direction;
            }
        }
    }

    private void SimulateMovement() {
        foreach (FakePlanet planet in planets) {
            planet.position += planet.velocity * Time.fixedDeltaTime * ticksPerStep;
        }
    }
}
