using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WorldForge.EditorTools
{
    /// <summary>Builds the few shapes Unity has no primitive for (torus, cone) and saves them as mesh assets.</summary>
    public static class MeshFactory
    {
        public const string Folder = "Assets/WorldForge/Meshes";

        public static Mesh Torus(float majorRadius, float minorRadius, int segments = 64, int sides = 12)
        {
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            for (int i = 0; i <= segments; i++)
            {
                float a = i / (float)segments * Mathf.PI * 2f;
                var centre = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * majorRadius;
                for (int j = 0; j <= sides; j++)
                {
                    float b = j / (float)sides * Mathf.PI * 2f;
                    var n = new Vector3(Mathf.Cos(a) * Mathf.Cos(b), Mathf.Sin(b), Mathf.Sin(a) * Mathf.Cos(b));
                    verts.Add(centre + n * minorRadius);
                    normals.Add(n);
                    uvs.Add(new Vector2(i / (float)segments, j / (float)sides));
                }
            }
            int stride = sides + 1;
            for (int i = 0; i < segments; i++)
            for (int j = 0; j < sides; j++)
            {
                int a = i * stride + j, b = a + stride;
                tris.AddRange(new[] { a, a + 1, b, b, a + 1, b + 1 });
            }
            return Save("Torus", verts, normals, uvs, tris);
        }

        /// <summary>Cone with its base at y=0 and apex at y=1, radius 0.5 (same footprint as a unit cylinder).</summary>
        public static Mesh Cone(int segments = 32)
        {
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            float slope = 0.5f;
            for (int i = 0; i <= segments; i++)
            {
                float a = i / (float)segments * Mathf.PI * 2f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                var n = (dir + Vector3.up * slope).normalized;
                verts.Add(dir * 0.5f); normals.Add(n); uvs.Add(new Vector2(i / (float)segments, 0f));
                verts.Add(Vector3.up); normals.Add(n); uvs.Add(new Vector2(i / (float)segments, 1f));
            }
            for (int i = 0; i < segments; i++)
            {
                int b = i * 2;
                tris.AddRange(new[] { b, b + 1, b + 2, b + 2, b + 1, b + 3 });
            }
            // Base cap
            int centre = verts.Count;
            verts.Add(Vector3.zero); normals.Add(Vector3.down); uvs.Add(new Vector2(0.5f, 0.5f));
            for (int i = 0; i <= segments; i++)
            {
                float a = i / (float)segments * Mathf.PI * 2f;
                verts.Add(new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 0.5f);
                normals.Add(Vector3.down);
                uvs.Add(new Vector2(0.5f + Mathf.Cos(a) * 0.5f, 0.5f + Mathf.Sin(a) * 0.5f));
            }
            for (int i = 0; i < segments; i++)
                tris.AddRange(new[] { centre, centre + 1 + i, centre + 2 + i });
            return Save("Cone", verts, normals, uvs, tris);
        }

        static Mesh Save(string name, List<Vector3> verts, List<Vector3> normals, List<Vector2> uvs, List<int> tris)
        {
            Directory.CreateDirectory(Folder);
            var mesh = new Mesh { name = name };
            mesh.SetVertices(verts);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            string path = $"{Folder}/{name}.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }
    }
}
