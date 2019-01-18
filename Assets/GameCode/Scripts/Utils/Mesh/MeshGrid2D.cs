using System;
using System.Collections.Generic;
using GameCode.Scripts.Utils.World;
using GameCode.Utils.Mesh;
using UnityEngine;

namespace GameCode.Scripts.Utils.Mesh
{
    public class MeshGrid2D
    {
        public static MeshData GetMeshGrid(Vector2Int startPos, Vector2Int gridStart, int gridSize, int cellSize,
            Func<Vector2Int, Color32> colorSupplier)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var colors = new List<Color32>();

            Debug.Log("Minimap with " + gridSize + " " + cellSize);

            for (var x = 0; x < gridSize; x++)
            {
                for (var y = 0; y < gridSize; y++)
                {
                    var color = colorSupplier.Invoke(new Vector2Int(x + gridStart.x, y + gridStart.y));

                    var index = vertices.Count;

                    var posX = x * cellSize + startPos.x;
                    var posY = y * cellSize + startPos.y;

                    vertices.Add(new Vector3(posX, posY, 0));
                    vertices.Add(new Vector3(posX, posY + cellSize, 0));
                    vertices.Add(new Vector3(posX + cellSize, posY, 0));
                    vertices.Add(new Vector3(posX + cellSize, posY + cellSize, 0));

                    triangles.Add(index);
                    triangles.Add(index + 2);
                    triangles.Add(index + 3);

                    triangles.Add(index + 3);
                    triangles.Add(index + 1);
                    triangles.Add(index);

                    colors.Add(color);
                    colors.Add(color);
                    colors.Add(color);
                    colors.Add(color);
                }
            }

            return new MeshData(vertices.ToArray(), colors.ToArray(), triangles.ToArray());
        }
    }
}