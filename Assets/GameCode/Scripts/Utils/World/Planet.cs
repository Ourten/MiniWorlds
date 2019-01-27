using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GameCode.Scripts.Utils.MathUtils;
using GameCode.Scripts.Utils.World.Biome;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace GameCode.Scripts.Utils.World
{
    public class PlanetConfig
    {
    }

    public class Planet
    {
        public float Radius { get; }
        public float Area { get; }
        public int Size { get; }
        public int Seed { get; }

        public int LowestPoint { get; private set; }
        public int HightestPoint { get; private set; }

        private const int HEIGHT = 16;
        private readonly Chunk[,,] _chunks;
        private readonly string _name;
        private readonly PlanetConfig _config;

        public int[,] HeightMap { get; set; }
        public float[,] MoistureMap { get; set; }
        public short[,] BiomeMap { get; set; }        

        public Planet(string name, int size, PlanetConfig config)
        {
            if (size % Chunk.SIZE != 0)
                throw new ArgumentException(nameof(size) + " must be divisible by " + Chunk.SIZE);

            _name = name;
            Size = size;
            _config = config;

            Area = size * size;
            Radius = size / 6.2832f;

            _chunks = new Chunk[size / Chunk.SIZE, HEIGHT, size / Chunk.SIZE];

            Seed = new Random().Next();
        }

        public GenerationState GeneratePlanet(Action afterGen)
        {
            var state = new GenerationState();
            state.Init(3);

            var continuedTask = Task.Run(() =>
                {
                    var watch = Stopwatch.StartNew();
                    watch.Start();

                    ChunkGenerator.Generate(this, state);

                    watch.Stop();
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        Debug.Log("Generation done in " + watch.ElapsedMilliseconds + "ms"));
                })
                .ContinueWith(task => state.Finish());

            if (afterGen != null)
                continuedTask.ContinueWith(task => afterGen.Invoke());

            return state;
        }

        public bool ChunkExist(int chunkX, int chunkY, int chunkZ)
        {
            return chunkX >= 0 && chunkY >= 0 && chunkZ >= 0 && chunkX < Size / Chunk.SIZE && chunkY < HEIGHT &&
                   chunkZ < Size / Chunk.SIZE;
        }

        public bool ChunkEmpty(int chunkX, int chunkY, int chunkZ)
        {
            return !ChunkExist(chunkX, chunkY, chunkZ) || _chunks[chunkX, chunkY, chunkZ] == null;
        }

        public List<ChunkView> GetChunksAround(Vector3Int origin, int radius)
        {
            var chunks = new List<ChunkView>();

            for (var x = origin.x - radius; x < origin.x + radius; x++)
            {
                for (var y = origin.y - radius; y < origin.y + radius; y++)
                {
                    for (var z = origin.z - radius; z < origin.z + radius; z++)
                    {
                        var offsetX = x;
                        var offsetZ = z;

                        if (x < 0)
                            offsetX = Size / Chunk.SIZE + x;
                        if (z < 0)
                            offsetZ = Size / Chunk.SIZE + z;
                        if (x >= Size / Chunk.SIZE)
                            offsetX = x - Size / Chunk.SIZE;
                        if (z >= Size / Chunk.SIZE)
                            offsetZ = z - Size / Chunk.SIZE;

                        if (PointUtils.IsPointInsideSphere(origin, x, y, z, radius) && !ChunkEmpty(offsetX, y, offsetZ))
                            chunks.Add(new ChunkView(_chunks[offsetX, y, offsetZ], x, y, z));
                    }
                }
            }

            return chunks;
        }

        private Chunk GetChunk(int chunkX, int chunkY, int chunkZ)
        {
            if (_chunks[chunkX, chunkY, chunkZ] == null)
                _chunks[chunkX, chunkY, chunkZ] = new Chunk(this, chunkX, chunkY, chunkZ);
            return _chunks[chunkX, chunkY, chunkZ];
        }

        public Chunk GetChunkFromChunkCoord(int chunkX, int chunkY, int chunkZ)
        {
            if (chunkX < 0 || chunkZ < 0 || chunkY < 0
                || chunkX >= _chunks.GetLength(0) || chunkY >= _chunks.GetLength(1) || chunkZ >= _chunks.GetLength(2))
                throw new ArgumentException("Illegal chunk coord [" + chunkX + " ; " + chunkY + " ; " + chunkZ);
            return GetChunk(chunkX, chunkY, chunkZ);
        }

        public Chunk GetChunkFromBlockCoord(int blockX, int blockY, int blockZ)
        {
            return GetChunkFromChunkCoord(blockX / Chunk.SIZE, blockY / Chunk.SIZE, blockZ / Chunk.SIZE);
        }

        public List<Chunk> GetChunks()
        {
            var chunks = new List<Chunk>();

            for (var x = 0; x < _chunks.GetLength(0); x++)
            {
                for (var y = 0; y < _chunks.GetLength(1); y++)
                {
                    for (var z = 0; z < _chunks.GetLength(2); z++)
                    {
                        if (_chunks[x, y, z] != null)
                            chunks.Add(_chunks[x, y, z]);
                    }
                }
            }

            return chunks;
        }

        public void SetBlock(int x, int y, int z, short value)
        {
            var chunk = GetChunkFromBlockCoord(x, y, z);

            if (value != 0)
            {
                if (y > HightestPoint)
                    HightestPoint = y;
                if (y < LowestPoint)
                    LowestPoint = y;
            }

            chunk.SetBlock(x % Chunk.SIZE, y % Chunk.SIZE, z % Chunk.SIZE, value);
        }

        public void SetArea(int xFrom, int yFrom, int zFrom, int xTo, int yTo, int zTo, short value)
        {
            for (var x = xFrom; x <= xTo; x++)
            {
                for (var y = yFrom; y <= yTo; y++)
                {
                    for (var z = zFrom; z <= zTo; z++)
                        SetBlock(x, y, z, value);
                }
            }
        }

        public short GetBlock(int x, int y, int z)
        {
            var chunk = GetChunkFromBlockCoord(x, y, z);

            return chunk.GetBlock(x % Chunk.SIZE, y % Chunk.SIZE, z % Chunk.SIZE);
        }

        public int GetHeightAt(int x, int z)
        {
            return HeightMap[x, z];
        }

        public short GetTopBlock(int x, int z)
        {
            return GetBlock(x, GetHeightAt(x, z), z);
        }

        public Biome.Biome GetBiomeAt(int x, int z)
        {
            return Biomes.ByID(BiomeMap[x, z]);
        }
    }
}