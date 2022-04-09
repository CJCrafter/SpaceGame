using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraMover))]
public class CameraMoverEditor : Editor {

    private CameraMover mover;

    private void OnEnable() {
        mover = (CameraMover) target;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        Camera cam = Camera.current;
        if (cam == null) {
            Debug.LogWarning("Null camera");
            return;
        }

        foreach (CameraMover.Position pos in mover.positions) {
            if (GUILayout.Button("Go to " + pos.position)) {
                cam.transform.position = pos.position;
                cam.transform.localRotation = pos.rotation;
            }
        }

        foreach (Planet planet in FindObjectsOfType<Planet>()) {
            if (GUILayout.Button("Spectate " + planet.name)) {
                cam.transform.parent = planet.transform;
                cam.transform.localPosition = new Vector3(0f, 0f, planet.terrain.radius * 2f);

                Vector3 between = planet.transform.position - cam.transform.position;
                cam.transform.localRotation = Quaternion.LookRotation(between);
            }
        }

        // When the camera has a parent, it is probably tracking a planet. We should have an 
        // option to separate the camera from the planet.
        if (cam.transform.parent != null) {
            if (GUILayout.Button("Separate from planet")) {
                cam.transform.parent = null;
            }
        }

        if (GUILayout.Button("Save current position")) {
            mover.positions.Add(new CameraMover.Position()
                {
                    position = cam.transform.position,
                    rotation = cam.transform.localRotation
                });
        }
    }
}
