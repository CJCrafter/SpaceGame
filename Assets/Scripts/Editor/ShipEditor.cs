using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ForceEntity), true), CanEditMultipleObjects]
public class ShipEditor : Editor {
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Save Mesh Data")) {
            ForceEntity ship = (ForceEntity) target;
            ForceEntity.MeshData mesh = new ForceEntity.MeshData(ship);
            mesh.Init();

            serializedObject.FindProperty("mesh").managedReferenceValue = mesh;
            serializedObject.ApplyModifiedProperties();
        }
    }
}