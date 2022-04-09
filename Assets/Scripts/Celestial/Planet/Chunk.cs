using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Chunk {

    private TerrainHandler terrain;
    public Mesh mesh;
    private int resolution;
    public Vector3 localUp;
    private Vector3 axisA;
    private Vector3 axisB;
    private Vector2 min;
    private Vector2 max;

    public Quaternion rotation;

    public Chunk(TerrainHandler terrain, Mesh mesh, int resolution, Vector3 localUp, Vector2 min, Vector2 max) {
        this.terrain = terrain;
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;
        this.min = min;
        this.max = max;

        // We need to form an axis using 3 perpendicular vectors
        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);

        // When we try to hide chunk faces, we need to know the
        // rotation of this chunk. Since the mesh has no rotation, 
        // we must calculate this value ourselves.
        Vector2 center = Vector2.Lerp(min, max, 0.5f);
        Vector3 cube = localUp + (center.x - 0.5f) * 2f * axisA + (center.y - 0.5f) * 2 * axisB;
        Vector3 sphere = cube.normalized;
        rotation = Quaternion.FromToRotation(sphere, Vector3.up);
    }

    public void Generate() {

        // (resolution - 1) * (resolution - 1) * 6 comes from the number of faces
        // on each side of the face * 3 points points per triangle * 2 triangles per face
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] indices = new int[(resolution - 1) * (resolution - 1) * 6];
        int triangleIndex = 0;
        Vector2[] uv = (mesh.uv.Length == vertices.Length) ? mesh.uv : new Vector2[vertices.Length];

        for (int y = 0; y < resolution; y++) {
            for (int x = 0; x < resolution; x++) {

                // A vector with components between [0, 1] 
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                int i = x + y * resolution;

                // Apply min/max to allow multiple meshes for each cube face. 
                // This is used to allow smaller meshes to be "skipped" during
                // rendering if they are facing the wrong way.
                // OldRange = (OldMax - OldMin) = 1 - 0
                // NewRange = (NewMax - NewMin) = max - min
                // NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin
                // 
                // So percent = ((percent - 0) * NewRange / 1) + min
                Vector2 adjusted = percent * (max - min) + min;
                //Debug.Log("Old: [0, 1] = " + percent + ", New: [" + min + ", " + max + "] " + adjusted);

                // Remember what a Terrainface is. It is a side of a cube. To
                // actually find those points, we need to use localUp to offset 
                // this face from the center of the cube. 
                Vector3 pointOnUnitCube = localUp + (adjusted.x - 0.5f) * 2f * axisA + (adjusted.y - 0.5f) * 2 * axisB;
                Vector3 pointOnUnitCircle = pointOnUnitCube.normalized;
                float elevation = terrain.CalculateUnscaledElevation(pointOnUnitCircle);
                vertices[i] = pointOnUnitCircle * terrain.CalculateScaledElevation(elevation);
                uv[i].y = elevation;

                // We need to create two clockwise triangles
                if (x != resolution - 1 && y != resolution - 1) {
                    indices[triangleIndex + 0] = i;
                    indices[triangleIndex + 1] = i + resolution + 1;
                    indices[triangleIndex + 2] = i + resolution;

                    indices[triangleIndex + 3] = i;
                    indices[triangleIndex + 4] = i + 1;
                    indices[triangleIndex + 5] = i + resolution + 1;
                    triangleIndex += 6;
                }
            }
        }

        // Clearing is optional, but may save a headache
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        mesh.uv = uv;
    }
}
