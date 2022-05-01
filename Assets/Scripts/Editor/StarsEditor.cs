using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Stars))]
public class StarsEditor : Editor {

    private Stars stars;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        
        if (GUILayout.Button("Generate Stars")) {
            stars.Generate();
        }
        
        if (GUILayout.Button("Generate File")) {
            stars.GenerateFile();
        }
    }
    
    private void OnEnable() {
        stars = (Stars) target;
    }

}
