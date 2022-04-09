using UnityEngine;

public class Planet : MonoBehaviour {

    [Range(1, 8), Tooltip("Divides cube into smaller meshes, scale with radius for best results")]
    public int chunks = 2;
    [Range(2, 256), Tooltip("Width of mesh, defines how many vertices are use (resolution * resolution)")]
    public int resolution = 8;
    [Range(0f, 180f), Tooltip("Hide backwards chunks, use >90.0f for best results")] 
    public float chunkAngle = 100.0f;

    [SerializeField, HideInInspector]
    private MeshFilter[] meshes;
    private Chunk[] faces;

    // Editor toggles todo init?
    public TerrainHandler terrain;
    public BiomeHandler biomes;
    public AtmosphereHandler atmosphere;

    // Other
    [Range(0f, 0.5f)]
    public float rotation;
    
    public void Init() {
        terrain.Update();
        // todo

        Vector3[] localUp =
            {
                Vector3.up, Vector3.back, Vector3.down, Vector3.forward, Vector3.left, Vector3.right
            };

        int total = 6 * chunks * chunks;

        // When the chunk count changes, we need to delete old meshes
        // before we regenerate them. Otherwise we would just keep adding
        // GameObjects over and over.
        if (meshes == null || meshes.Length != total || faces == null) {
            for (int i = transform.childCount - 1; i >= 0; i--) {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            meshes = new MeshFilter[total];
        }
        else if (meshes[0].sharedMesh.vertexCount == resolution * resolution) {
            return; // Experimental
        }

        Debug.Log("Update");
        faces = new Chunk[total];
        int meshesPerFace = chunks * chunks;
        for (int i = 0; i < total; i++) {
            Vector3 up = localUp[i / meshesPerFace];
            int y = (i % meshesPerFace) / chunks % chunks;
            int x = (i % meshesPerFace) % chunks;
            //Debug.Log(i + ": (" + x + ", " + y + ")");

            if (meshes[i] == null) {
                GameObject obj = new GameObject("Mesh_" + i);
                obj.transform.parent = transform;
                obj.transform.localPosition = Vector3.zero;

                obj.AddComponent<MeshRenderer>();
                meshes[i] = obj.AddComponent<MeshFilter>();
                meshes[i].sharedMesh = new Mesh();
            }

            meshes[i].GetComponent<MeshRenderer>().sharedMaterial = biomes.material;
            Vector2 min = new Vector2(x, y) / chunks;
            Vector2 max = new Vector2(x + 1, y + 1) / chunks;
            faces[i] = new Chunk(terrain, meshes[i].sharedMesh, resolution, up, min, max);
        }
    }

    public void Generate() {
        Init();
        GenerateMeshes();
        // todo
    }

    private void GenerateMeshes() {
        foreach (Chunk chunk in faces)
            chunk.Generate();
    }

    public void UpdateTerrain() {
        Init();
        GenerateMeshes();
    }

    public void UpdateBiomes() {
        Init();
        //biomes.Update();
    }

    public void UpdateAtmosphere() {
        //atmosphere.Update();
    }

    private void Update() {

        transform.localRotation *= Quaternion.AngleAxis(rotation, Vector3.up);

        if (false) {
            // Chunks should only be shown if the angle between their normal
            // and the camera is less then chunkAngle. This will improve GPU performance
            Camera cam = Camera.current;
            if (cam == null)
                cam = FindObjectOfType<Camera>();

            // Planet has not yet been generated.
            if (faces == null)
                return;

            for (var i = 0; i < faces.Length; i++) {
                float angle = Quaternion.Angle(cam.transform.rotation, faces[i].rotation);
                meshes[i].gameObject.SetActive(angle < chunkAngle);
            }
        }
    }
}
