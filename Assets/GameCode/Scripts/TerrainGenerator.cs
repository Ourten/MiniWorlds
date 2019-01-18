using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GameCode.Scripts.Utils.Mesh;
using GameCode.Scripts.Utils.World;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class TerrainGenerator : MonoBehaviour
{
    public int mapSize = 512;
    public int viewRadius = 8;
    public GameObject player;
    public bool meshAll;
    public Text textStatus;

    private Vector3Int playerLastChunk;

    private Planet _planet;

    private GameObject _mainGameObject;
    private Dictionary<ChunkView, GameObject> meshedChunks;

    private GenerationState _genState;

    // Start is called before the first frame update
    void Start()
    {
        if (mapSize < viewRadius * 16)
            throw new ArgumentException(nameof(mapSize) + " (" + mapSize + ") cannot be inferior to the " +
                                        nameof(viewRadius) + " (" + viewRadius * 16 + ")");

        World.Instance.AddPlanet("Spawn", mapSize, new PlanetConfig());
        _planet = World.Instance.GetPlanet("Spawn");

        _mainGameObject = new GameObject();
        meshedChunks = new Dictionary<ChunkView, GameObject>();

        if (!meshAll)
            _genState = _planet.GeneratePlanet(null);
        else
            _genState = _planet.GeneratePlanet(() =>
            {
                _planet.GetChunks().AsParallel().AsOrdered().ForAll(chunk =>
                {
                    var meshData = MeshCreator.ReduceMesh(chunk);

                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        var childGameObject = new GameObject("Grid ");
                        childGameObject.AddComponent<MeshFilter>();
                        childGameObject.AddComponent<MeshRenderer>();

                        childGameObject.GetComponent<MeshRenderer>().material.shader =
                            Shader.Find("Custom/StandardColor");
                        childGameObject.isStatic = true;
                        childGameObject.transform.parent = _mainGameObject.transform.parent;
                        childGameObject.transform.Translate(new Vector3(chunk.ChunkX, chunk.ChunkY, chunk.ChunkZ) *
                                                            Chunk.SIZE);

                        Mesh mesh;
                        childGameObject.GetComponent<MeshFilter>().mesh = mesh = new Mesh();
                        mesh.name = "Procedural Grid " + chunk.ChunkX + "x" + chunk.ChunkZ;

                        mesh.vertices = meshData.Vertices;
                        mesh.colors32 = meshData.Colors;
                        mesh.triangles = meshData.Triangles;

                        mesh.RecalculateNormals();
                        mesh.RecalculateBounds();

                        childGameObject.AddComponent<MeshCollider>();
                    });
                });
            });
    }

    private void ShiftWorld(Vector3 toShift)
    {
        player.transform.position += toShift;

        foreach (var goChunk in meshedChunks.Values)
            goChunk.transform.position += toShift;
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = 0;

        if (!_genState.IsFinished)
        {
            textStatus.text =
                $"Creating World...\nStep {_genState.CurrentStep + 1} of {_genState.MaxStep}\n{_genState.StepDesc} : {(float) _genState.CurrentStepProgress / _genState.CurrentStepMaxProgress:P}";
            return;
        }

        textStatus.text = "";
        Time.timeScale = 1;

        if (meshAll)
            return;

        var playerPos = player.transform.position;
        var playerCurrentChunk = Vector3Int.RoundToInt(playerPos / 16);

        if (playerPos.x < 0)
            ShiftWorld(new Vector3(_planet.Size, 0, 0));
        if (playerPos.z < 0)
            ShiftWorld(new Vector3(0, 0, _planet.Size));
        if (playerPos.x >= _planet.Size)
            ShiftWorld(new Vector3(-_planet.Size, 0, 0));
        if (playerPos.z >= _planet.Size)
            ShiftWorld(new Vector3(0, 0, -_planet.Size));

        if (playerCurrentChunk.Equals(playerLastChunk))
            return;

        var watch = Stopwatch.StartNew();
        watch.Start();
        playerLastChunk = playerCurrentChunk;

        var chunkList = _planet.GetChunksAround(playerCurrentChunk, viewRadius);
        var toEvict = new BlockingCollection<ChunkView>();
        var notAdded = new BlockingCollection<ChunkView>();

        Debug.Log("Get Chunk Around " + watch.ElapsedMilliseconds + "ms");
        watch.Restart();

        meshedChunks.Keys.AsParallel().ForAll(view =>
        {
            var candidate = chunkList.FirstOrDefault(toAdd => toAdd.Chunk.Equals(view.Chunk));

            if (candidate == null)
                toEvict.Add(view);
            else
                notAdded.Add(candidate);
        });

        Debug.Log("MeshedLoop " + watch.ElapsedMilliseconds + "ms " + meshedChunks.Count);

        foreach (var chunk in toEvict)
        {
            Destroy(meshedChunks[chunk]);
            meshedChunks.Remove(chunk);
        }

        foreach (var chunk in notAdded)
            chunkList.Remove(chunk);

        Task.Run(() =>
        {
            chunkList.AsParallel().AsOrdered().ForAll(chunkView =>
            {
                var meshData = MeshCreator.ReduceMesh(chunkView.Chunk);

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    var childGameObject = new GameObject("Grid ");
                    childGameObject.AddComponent<MeshFilter>();
                    childGameObject.AddComponent<MeshRenderer>();

                    childGameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("Custom/StandardColor");
                    childGameObject.isStatic = true;
                    childGameObject.transform.parent = _mainGameObject.transform.parent;
                    childGameObject.transform.Translate(new Vector3(chunkView.X, chunkView.Y, chunkView.Z) *
                                                        Chunk.SIZE);

                    Mesh mesh;
                    childGameObject.GetComponent<MeshFilter>().mesh = mesh = new Mesh();
                    mesh.name = "Procedural Grid " + chunkView.X + "x" + chunkView.Z;

                    mesh.vertices = meshData.Vertices;
                    mesh.colors32 = meshData.Colors;
                    mesh.triangles = meshData.Triangles;

                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();

                    childGameObject.AddComponent<MeshCollider>();

                    meshedChunks.Add(chunkView, childGameObject);
                });
            });
        });
    }
}