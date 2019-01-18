using System;
using System.Collections.Generic;
using GameCode.Scripts.Gui;
using GameCode.Scripts.Utils;
using GameCode.Scripts.Utils.Mesh;
using GameCode.Scripts.Utils.World;
using GameCode.Scripts.Utils.World.Biome;
using GameCode.Utils.Mesh;
using UnityEngine;

public class Minimap : MonoBehaviour, IGui
{
    public Material material;

    private CanvasRenderer _canvasRenderer;

    private GameObject[] mapParts;

    private void Start()
    {
        _canvasRenderer = GetComponent<CanvasRenderer>();
        _canvasRenderer.SetMaterial(material, null);

        mapParts = new GameObject[16];
    }

    private void Update()
    {
    }

    private readonly Color32[] _gradient = {new Color32(226, 234, 4, 255), new Color32(234, 23, 4, 255)};

    public void OnOpen()
    {
        var planet = World.Instance.GetPlanet("Spawn");
        var scale = 256f / planet.Size;
        var ratio = (int) Math.Floor(1 / scale);

        Debug.Log("Meshing minimap with scale " + scale + " and ratio " + ratio);

        Color32 BiomeColorSupplier(Vector2Int pos)
        {
            var colors = new List<Color32>();

            for (var x = 0; x < ratio; x++)
            {
                for (var y = 0; y < ratio; y++)
                    colors.Add(planet.GetBiomeAt(pos.x * ratio + x, pos.y * ratio + y).Color);
            }

            return ColorUtils.GetAverageRGB(colors);
        }

        Color32 ElevationColorSupplier(Vector2Int pos)
        {
            var colors = new List<Color32>();
            var lowest = planet.LowestPoint;
            var hightest = planet.HightestPoint;

            for (var x = 0; x < ratio; x++)
            {
                for (var y = 0; y < ratio; y++)
                    colors.Add(Color32.Lerp(_gradient[0], _gradient[1],
                        (float) (planet.GetHeightAt(pos.x * ratio + x, pos.y * ratio + y) - lowest) /
                        (hightest - lowest)));
            }

            return ColorUtils.GetAverageRGB(colors);
        }

        for (var column = 0; column < 4; column++)
        {
            for (var row = 0; row < 4; row++)
            {
                var mapPart = new GameObject {name = "MapPart " + column + " " + row};

                var mesh = MeshGrid2D.GetMeshGrid(new Vector2Int(-256 + column * 128, -256 + row * 128),
                        new Vector2Int(planet.Size / ratio / 4 * column, planet.Size / ratio / 4 * row),
                        64, 2, ElevationColorSupplier)
                    .ToUnityMesh();

                mapPart.AddComponent<CanvasRenderer>().SetMesh(mesh);
                mapPart.GetComponent<CanvasRenderer>().SetMaterial(material, null);
                mapPart.transform.parent = transform;
                mapPart.transform.localScale = Vector3.one;
                mapPart.transform.position = transform.position;

                mapParts[column + 4 * row] = mapPart;
            }
        }

        Debug.Log("Meshes done");
    }

    public void OnClose()
    {
        foreach (var mapPart in mapParts)
            Destroy(mapPart);
    }
}