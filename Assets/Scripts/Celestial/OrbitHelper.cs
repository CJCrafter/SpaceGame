using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class OrbitHelper : MonoBehaviour {

    private class EntitySimulation {
        internal readonly ForceEntity entity;
        internal GameObject gameObject;
        internal ShowOrbit draw;
        internal Vector3 position;
        internal Vector3 velocity;
        internal float mass;
        internal bool hasGravity;
        internal float radius;

        internal EntitySimulation(ForceEntity entity) {
            this.entity = entity;
            Update();
        }

        internal void Update() {
            gameObject = entity.gameObject;
            draw = gameObject.GetComponent<ShowOrbit>();
            position = entity.transform.position;
            velocity = Application.isPlaying ? entity.body.velocity : entity.initialVelocity;
            mass = entity.mass;
            hasGravity = entity.hasGravity;
            radius = entity.mesh?.radius ?? 5f;
        }
    }

    [Min(1)] public int steps = 100;
    [Min(1f)] public float timeStep = 1f;
    [Min(1f)] public float updateInterval = 1f;
    public bool thin = true;
    public ForceEntity relative;

    private float lastUpdate;
    private EntitySimulation relativeCache;
    private List<EntitySimulation> entities;
    public List<string> names;


    public void OnEnable() {
        foreach (ShowOrbit orbit in FindObjectsOfType<ShowOrbit>())
            orbit.enabled = true;
    }

    public void OnDisable() {
        foreach (ShowOrbit orbit in FindObjectsOfType<ShowOrbit>())
            orbit.enabled = false;
    }

    public void Init() {
        entities = new List<EntitySimulation>();
        names = new List<string>();

        foreach (ForceEntity entity in FindObjectsOfType<ForceEntity>()) {
            EntitySimulation simulation = new EntitySimulation(entity);
            entities.Add(simulation);
            names.Add(entity.name);
        }
    }

    private void OnValidate() {
        Init();
        relativeCache = null;
        foreach (ShowOrbit orbit in FindObjectsOfType<ShowOrbit>()) {
            orbit.useGL = !thin;
        }

        ShowOrbits();
    }

    private void Update() {
        if (lastUpdate > Time.timeSinceLevelLoad + updateInterval)
            return;
        
        ShowOrbits();
        lastUpdate = Time.timeSinceLevelLoad;   
    }

    public void ShowOrbits() {
        if (entities == null)
            Init();

        foreach (EntitySimulation entity in entities) {
            entity.Update();
        }

        var skip = new bool[entities.Count];
        var points = new Vector3[entities.Count, steps];
        
        Vector3 initialReferenceFrame = GetRelativeVector();
        for (int i = 0; i < steps; i++) {
            Vector3 offset = GetRelativeVector() - initialReferenceFrame;

            // Physics calculations... Pretty heavy. 
            SimulateGravity(skip);
            SimulateMovement(skip);

            for (int j = 0; j < entities.Count; j++) {
                if (skip[j])
                    continue;
                
                Vector3 current = entities[j].position;
                if (entities[j] != relativeCache)
                    current -= offset;

                // Save current location of each planet
                points[j, i] = current;
                
                // Additionally, lets check if we are in a loop. This means
                // we are in a stable orbit and can stop calculations for this
                // specific planet.
                if (j - i > 10 && MathUtil.SquareDistance(points[j, i], points[j, 0]) < 25f)
                    skip[j] = true;
            }
        }

        // Setting the points causes the line renderer to do work.
        for (int i = 0; i < entities.Count; i++) {
            
            Vector3[] temp = new Vector3[steps];
            for (int j = 0; j < steps; j++)
                temp[j] = points[i, j];
            
            entities[i].draw.points = temp;
        }
    }

    private Vector3 GetRelativeVector() {
        if (relativeCache == null && relative != null) 
            relativeCache = entities.Where(entity => entity.gameObject == relative.gameObject).GetEnumerator().Current;
        
        return relativeCache?.position ?? Vector3.zero;
    }

    private void SimulateGravity(bool[] skip) {
        foreach (EntitySimulation gravity in entities) {
            if (!gravity.hasGravity)
                continue;

            for (var i = 0; i < entities.Count; i++) {
                EntitySimulation entity = entities[i];
                if (entity == gravity)
                    continue;
                
                // Slight optimization to stop calculating gravity for 
                // objects in a stable orbit OR if the orbit collided with
                // a planet.
                if (!entity.hasGravity && skip[i])
                    continue;
                
                Vector3 direction = gravity.position - entity.position;

                float distanceSquared = direction.sqrMagnitude;
                if (distanceSquared < gravity.radius) {
                    skip[i] = true;
                    continue;
                }

                float force = Universe.gravitationalConstant * gravity.mass / distanceSquared;

                direction.Normalize();
                direction *= force * timeStep; // Use Time.deltaTime here if wanted. We just always simulate 1 second
                entity.velocity += direction;
            }
        }
    }

    private void SimulateMovement(bool[] skip) {
        for (var i = 0; i < entities.Count; i++) {
            EntitySimulation entity = entities[i];
            if (skip[i] && !entity.hasGravity)
                continue;
            
            entity.position += entity.velocity * timeStep;
        }
    }

}