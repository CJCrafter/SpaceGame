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
        internal Vector3 position;
        internal Vector3 velocity;
        internal float mass;
        internal bool hasGravity;
        internal bool usesGravity;
        internal Color color; 

        internal FakePlanet(GravityObject obj) {
            name = "Fake " + obj.name;
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
    }

    // Stores colors for each map
    private static SortedDictionary<string, Color> colorMap = new SortedDictionary<string, Color>();
    private static Color[] colors =
        {
            Color.blue, Color.cyan, Color.gray, Color.green, Color.magenta, Color.red, Color.yellow, 
            new Color(1f, 1f, 0f), new Color(0.5f, 0f, 1f)
        };

    [Min(0.00001f)]
    public float timeStep = 60f * 60f;
    [Min(1)]
    public int steps = 100;

    public bool show = true;
    public bool regenerate = true;
    public GravityObject relative;

    private List<FakePlanet> planets;
    public List<string> planetNames;

    public void Init() {

        // Start by clearing out old planets
        planets = new List<FakePlanet>();
        planetNames = new List<string>();

        // We need to wrap each planet it a "FakePlanet" so we
        // don't accidentally modify an existing planet.
        foreach (GravityObject obj in FindObjectsOfType<GravityObject>()) {
            FakePlanet fake = new FakePlanet(obj);
            planets.Add(fake);
            planetNames.Add(fake.name);
        }
    }

    private void OnValidate() {
        Update();
        regenerate = false;
    }

    private void Update() {
        Init();
        ShowOrbits();
    }

    public void ShowOrbits() {
        if (!show || planets == null || !planets.Any())
            return;

        Universe universe = FindObjectOfType<Universe>();
        if (universe == null)
            throw new MissingUniverseException();

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
        if (relative == null)
            return Vector3.zero;

        string target = "Fake " + relative.gameObject.name;
        foreach (FakePlanet planet in planets) {
            if (planet.name == target)
                return planet.position;
        }

        throw new Exception("Cannot find: " + target);
    }

    private void SimulateGravity() {
        foreach (FakePlanet planet in planets)
        {
            if (!planet.usesGravity)
                continue;

            foreach (FakePlanet gravityEmitter in planets) {
                if (!gravityEmitter.hasGravity)
                    continue;

                Vector3 direction = gravityEmitter.position - planet.position;

                float distanceSquared = direction.sqrMagnitude;
                if (distanceSquared == 0.0f)
                    continue;

                float force = Universe.gravitationalConstant * (planet.mass * gravityEmitter.mass) / distanceSquared;

                direction.Normalize();
                direction *= force / planet.mass * timeStep;
                planet.velocity += direction;
            }
        }
    }

    private void SimulateMovement() {
        foreach (FakePlanet planet in planets) {
            planet.position += planet.velocity * timeStep;
        }
    }
}
