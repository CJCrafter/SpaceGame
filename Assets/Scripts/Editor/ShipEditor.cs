using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ForceEntity), true)]
public class EntityEditor : Editor {

    private ForceEntity entity;
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Save Mesh Data")) {
            ForceEntity.MeshData mesh = new ForceEntity.MeshData(entity);
            mesh.Init();

            // Doing this allows us to undo actions
            serializedObject.FindProperty("mesh").managedReferenceValue = mesh;
            serializedObject.ApplyModifiedProperties();
        }

        if (GUILayout.Button("Attempt Orbit")) {
            ForceEntity strongest = entity.strongestGravity;
            if (strongest == null) {
                float max = float.MinValue;
                foreach (ForceEntity gravity in FindObjectsOfType<ForceEntity>()) {
                    if (!gravity.hasGravity || entity == gravity)
                        continue;
                    
                    Vector3 between = gravity.transform.position - entity.transform.position;
                    float distanceSquared = between.sqrMagnitude;
                    float force = Universe.gravitationalConstant * gravity.mass / distanceSquared;

                    if (force > max) {
                        max = force;
                        strongest = gravity;
                    }
                }
            }
            
            if (strongest == null) {
                Debug.LogWarning("Couldn't find strongest gravity");
            } else {
                   
                // In order to 'guess' our orbit relative to our parent, we determine
                // the force of gravity and the centripetal force to match
                // mv^2 / r = G * m1 * m2 / r^2
                // v^2 = G  * m2 / r
                // v = sqrt(G * m2 / r)
                Vector3 direction = strongest.transform.position - entity.transform.position;
                float velocity = Mathf.Sqrt(Universe.gravitationalConstant * strongest.mass / direction.magnitude);
                
                // Choose which direction to go in
                Vector3 vector = entity.initialVelocity;
                if (vector == Vector3.zero)
                    vector = Vector3.Cross(direction.normalized, Vector3.up);

                serializedObject.FindProperty("initialVelocity").vector3Value = vector.normalized * velocity;
                serializedObject.ApplyModifiedProperties();
                Debug.Log("Set velocity relative to " + strongest.gameObject.name);
            }
        }
    }

    private void OnEnable() {
        entity = (ForceEntity) target;
    }
}