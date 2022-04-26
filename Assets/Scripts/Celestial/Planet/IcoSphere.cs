using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

public static class Icosphere {

    public struct Point {
        public readonly Vector3 normal;
        public float elevation;

        public Point(Vector3 normal) {
            this.normal = normal;
            elevation = 0f;
        }
    }
    
    private struct TriangleIndices {
        public int v1;
        public int v2;
        public int v3;

        public TriangleIndices(int v1, int v2, int v3) {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }

    // return index of point in the middle of p1 and p2
    private static int getMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<long, int> cache, float radius) {
        // first check if we have it already
        bool firstIsSmaller = p1 < p2;
        long smallerIndex = firstIsSmaller ? p1 : p2;
        long greaterIndex = firstIsSmaller ? p2 : p1;
        long key = (smallerIndex << 32) + greaterIndex;

        int ret;
        if (cache.TryGetValue(key, out ret)) {
            return ret;
        }

        // not in cache, calculate it
        Vector3 point1 = vertices[p1];
        Vector3 point2 = vertices[p2];
        Vector3 middle = new Vector3(
            (point1.x + point2.x) / 2f,
            (point1.y + point2.y) / 2f,
            (point1.z + point2.z) / 2f
        );

        // add vertex makes sure point is on unit sphere
        int i = vertices.Count;
        vertices.Add(middle.normalized * radius);

        // store it, return index
        cache.Add(key, i);

        return i;
    }

    public static void Create(GameObject obj, int recursionLevel, float radius) {
        if (obj.GetComponent<MeshRenderer>() == null) obj.AddComponent<MeshRenderer>();
        if (obj.GetComponent<MeshFilter>() == null) obj.AddComponent<MeshFilter>();
        
        MeshFilter filter = obj.GetComponent<MeshFilter>();
        Mesh mesh = filter.sharedMesh;
        mesh.Clear();
        
        var middlePointIndexCache = new Dictionary<long, int>();
        float t = (1f + Mathf.Sqrt(5f)) / 2f;
        var vertList = new List<Vector3>
            {
                new Vector3(-1f, t, 0f).normalized * radius,
                new Vector3(1f, t, 0f).normalized * radius,
                new Vector3(-1f, -t, 0f).normalized * radius,
                new Vector3(1f, -t, 0f).normalized * radius,
                new Vector3(0f, -1f, t).normalized * radius,
                new Vector3(0f, 1f, t).normalized * radius,
                new Vector3(0f, -1f, -t).normalized * radius,
                new Vector3(0f, 1f, -t).normalized * radius,
                new Vector3(t, 0f, -1f).normalized * radius,
                new Vector3(t, 0f, 1f).normalized * radius,
                new Vector3(-t, 0f, -1f).normalized * radius,
                new Vector3(-t, 0f, 1f).normalized * radius
            };


        // create 20 triangles of the icosahedron
        var faces = new List<TriangleIndices>
            {
                // 5 faces around point 0
                new TriangleIndices(0, 11, 5),
                new TriangleIndices(0, 5, 1),
                new TriangleIndices(0, 1, 7),
                new TriangleIndices(0, 7, 10),
                new TriangleIndices(0, 10, 11),
                // 5 adjacent faces
                new TriangleIndices(1, 5, 9),
                new TriangleIndices(5, 11, 4),
                new TriangleIndices(11, 10, 2),
                new TriangleIndices(10, 7, 6),
                new TriangleIndices(7, 1, 8),
                // 5 faces around point 3
                new TriangleIndices(3, 9, 4),
                new TriangleIndices(3, 4, 2),
                new TriangleIndices(3, 2, 6),
                new TriangleIndices(3, 6, 8),
                new TriangleIndices(3, 8, 9),
                // 5 adjacent faces
                new TriangleIndices(4, 9, 5),
                new TriangleIndices(2, 4, 11),
                new TriangleIndices(6, 2, 10),
                new TriangleIndices(8, 6, 7),
                new TriangleIndices(9, 8, 1)
            };


        // refine triangles
        for (int i = 0; i < recursionLevel; i++) {
            var faces2 = new List<TriangleIndices>();
            foreach (var tri in faces) {
                // replace triangle by 4 triangles
                int a = getMiddlePoint(tri.v1, tri.v2, ref vertList, ref middlePointIndexCache, radius);
                int b = getMiddlePoint(tri.v2, tri.v3, ref vertList, ref middlePointIndexCache, radius);
                int c = getMiddlePoint(tri.v3, tri.v1, ref vertList, ref middlePointIndexCache, radius);

                faces2.Add(new TriangleIndices(tri.v1, a, c));
                faces2.Add(new TriangleIndices(tri.v2, b, a));
                faces2.Add(new TriangleIndices(tri.v3, c, b));
                faces2.Add(new TriangleIndices(a, b, c));
            }

            faces = faces2;
        }

        mesh.vertices = vertList.ToArray();

        var triList = new List<int>();
        for (int i = 0; i < faces.Count; i++) {
            triList.Add(faces[i].v1);
            triList.Add(faces[i].v2);
            triList.Add(faces[i].v3);
        }

        mesh.triangles = triList.ToArray();
        mesh.uv = new Vector2[vertList.Count];

        var normals = new Vector3[vertList.Count];
        for (int i = 0; i < normals.Length; i++)
            normals[i] = vertList[i].normalized;

        mesh.normals = normals;
        
        mesh.RecalculateBounds();
        mesh.Optimize();
    }
}