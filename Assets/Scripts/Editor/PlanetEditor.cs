using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor {
    
    private Planet planet;
    private bool collapseTerrain;
    private bool collapseBiomes;
    private bool collapseAtmosphere;

    // Cache editors so we don't regenerate them
    private Editor terrainEditor;
    private Editor biomesEditor;
    private Editor atmosphereEditor;

    public override void OnInspectorGUI() {
        using (var check = new EditorGUI.ChangeCheckScope()) {
            base.OnInspectorGUI();
            if (check.changed) {
                planet.Generate();
            }
        }

        if (GUILayout.Button("Generate Planet")) {
            planet.Generate();
        }

        DrawSettingsEditor(planet.terrain, planet.UpdateTerrain, ref collapseTerrain, ref terrainEditor);
        DrawSettingsEditor(planet.biomes, planet.UpdateBiomes, ref collapseBiomes, ref biomesEditor);
        DrawSettingsEditor(planet.atmosphere, planet.UpdateAtmosphere, ref collapseAtmosphere, ref atmosphereEditor);
    }

    void DrawSettingsEditor(Object settings, System.Action updateMethod, ref bool foldout, ref Editor editor)
    {
        if (settings == null)
            return;

        foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
        using var check = new EditorGUI.ChangeCheckScope();
        if (foldout) {
            CreateCachedEditor(settings, null, ref editor);
            editor.OnInspectorGUI();

            if (check.changed && updateMethod != null) {
                updateMethod();
            }
        }
    }

    private void OnEnable()
    {
        planet = (Planet)target;
    }
    // MenuItems allow us to add options to the editor in general, instead
    // of adding stuff to the inspector for each gameobject.
    [MenuItem("GameObject/Celestial/Create Planet")]
    public static void CreatePlanet() {
        PlanetPopup window = CreateInstance<PlanetPopup>();
        window.position = new Rect(Screen.width / 2f, Screen.height / 2f, 500, 300);
        window.ShowPopup();
    }


}

public class PlanetPopup : EditorWindow {

    private float scale = 100f;

    private GravityObject parent;
    private bool attemptStableOrbit = true;
    private float distance = 200f;

    // 0 = none
    // 1 = base planet
    // 2 = earthlike
    // todo add more
    private int generator;

    private void OnGUI() {

        scale = EditorGUILayout.Slider("Size", scale, 1f, 1000f);
        parent = (GravityObject) EditorGUILayout.ObjectField("Relative", parent, typeof(GravityObject), true);
        attemptStableOrbit = EditorGUILayout.Toggle("Try Orbit", attemptStableOrbit);
        distance = EditorGUILayout.Slider("Distance", distance, 1.0f, 10000f);

        generator = EditorGUILayout.Popup("Generator", generator, new string[] {"none", "flat", "earth"});

        if (GUILayout.Button("Create")) {
            GameObject obj = new GameObject("Generated Planet");
            GravityObject planet = obj.AddComponent<GravityObject>();
            planet.mass = 100000f * scale;
            planet.hasGravity = true;
            planet.usesGravity = true;
            obj.transform.position = new Vector3(-distance, 0f, 0f);// todo randomize start

            if (attemptStableOrbit) {

                obj.transform.position += parent.transform.position;

                // In order to 'guess' our orbit relative to our parent, we determine
                // the force of gravity and the centripetal force to match
                Vector3 direction = parent.transform.position - planet.transform.position;
                float velocity = Mathf.Sqrt(Universe.gravitationalConstant * parent.mass / direction.magnitude);
                planet.velocity = new Vector3(0f, 0f, velocity);
            }

            if (generator == 1) {
                Planet generation = obj.AddComponent<Planet>();
            }
        }

        if (GUILayout.Button("Close")) {
            Close();
        }
    }
}
