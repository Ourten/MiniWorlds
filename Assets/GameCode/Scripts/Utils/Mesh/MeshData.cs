using System;
using System.Linq;
using UnityEngine;

namespace GameCode.Utils.Mesh
{
    public class MeshData
    {
        public Vector3[] Vertices { get; }

        public Color32[] Colors { get; }

        public int[] Triangles { get; }

        public MeshData(Vector3[] vertices, Color32[] colors, int[] triangles)
        {
            Vertices = vertices;
            Colors = colors;
            Triangles = triangles;
        }

        public static MeshData operator +(MeshData first, MeshData second)
        {
            var mergedTriangles = new int[first.Triangles.Length + second.Triangles.Length];

            Array.Copy(first.Triangles, mergedTriangles, first.Triangles.Length);

            for (var i = 0; i < second.Triangles.Length; i++)
                mergedTriangles[i + first.Triangles.Length] = first.Vertices.Length + second.Triangles[i];

            return new MeshData(first.Vertices.Concat(second.Vertices).ToArray(),
                first.Colors.Concat(second.Colors).ToArray(), mergedTriangles);
        }
    }
}