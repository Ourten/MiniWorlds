using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GameCode.Scripts.Utils.MathUtils;
using UnityEngine;
using Random = System.Random;

namespace GameCode.Scripts.Utils.World
{
    public class World
    {
        public float Radius { get; }
        public float Area { get; }
        public int Size { get; }
        public int Seed { get; }

        private const int HEIGHT = 16;
        private readonly Chunk[,,] _chunks;

        private int _verticesCount;
        private int _trianglesCount;

        public int VerticesCount
        {
            get => _verticesCount;
            set => Interlocked.Add(ref _verticesCount, value);
        }

        public int TrianglesCount
        {
            get => _trianglesCount;
            set => Interlocked.Add(ref _trianglesCount, value);
        }

        public World(int size)
        {
            if (size % Chunk.SIZE != 0)
                throw new ArgumentException(nameof(size) + " must be divisible by " + Chunk.SIZE);

            Size = size;

            Area = size * size;
            Radius = size / 6.2832f;

            _chunks = new Chunk[size / Chunk.SIZE, HEIGHT, size / Chunk.SIZE];

            Seed = new Random().Next();
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
    }

    public static class WorldGenerator
    {
        private static FastNoise _noise = new FastNoise();

        static WorldGenerator()
        {
            _noise.SetNoiseType(FastNoise.NoiseType.Perlin);
        }

        public static void GenerateWorld(World world)
        {
            var heightMap = CreateWorldHeightMap(world.Size, world.Seed);

            for (var x = 0; x < world.Size; x++)
            {
                for (var z = 0; z < world.Size; z++)
                {
                    world.SetArea(x, 0, z, x, heightMap[x, z] - 1, z, 1);
                    world.SetBlock(x, heightMap[x, z], z, 2);
                }
            }
        }

        private static int[,] CreateWorldHeightMap(int size, int seed)
        {
            var worldHeightMap = JoinHeightMaps(
                Enumerable.Range(0, (int) System.Math.Pow((double) size / Chunk.SIZE, 2)).AsParallel().AsOrdered()
                    .Select(chunkIndex => GenMap(size, chunkIndex, Chunk.SIZE, seed))
                    .ToList(), size, Chunk.SIZE);

            return worldHeightMap;
        }

        private static int[,] JoinHeightMaps(IReadOnlyList<int[,]> maps, int size, int chunkSize)
        {
            var heightMap = new int[size, size];

            for (var index = 0; index < maps.Count; index++)
            {
                var chunkX = index % (size / chunkSize) * chunkSize;
                var chunkY = index / (size / chunkSize) * chunkSize;

                for (var x = 0; x < chunkSize; x++)
                {
                    for (var y = 0; y < chunkSize; y++)
                        heightMap[chunkX + x, chunkY + y] = maps[index][x, y];
                }
            }

            return heightMap;
        }


        private static int[,] GenMap(int size, int chunkIndex, int chunkSize, int seed)
        {
            var heightMap = new int[chunkSize, chunkSize];
            var chunkX = chunkIndex % (size / chunkSize) * chunkSize;
            var chunkY = chunkIndex / (size / chunkSize) * chunkSize;

            _noise.SetSeed(seed);

            int x1 = -size / 2, y1 = -size / 2, x2 = size / 2, y2 = size / 2;
            var dx = x2 - x1;
            var dy = y2 - y1;

            for (var y = 0; y < chunkSize; y++)
            {
                for (var x = 0; x < chunkSize; x++)
                {
                    var s = (float) (chunkX + x) / size;
                    var t = (float) (chunkY + y) / size;

                    var nx = x1 + Math.Cos(s * 2 * Math.PI) * dx / (2 * Math.PI);
                    var ny = y1 + Math.Cos(t * 2 * Math.PI) * dy / (2 * Math.PI);
                    var nz = x1 + Math.Sin(s * 2 * Math.PI) * dx / (2 * Math.PI);
                    var nw = y1 + Math.Sin(t * 2 * Math.PI) * dy / (2 * Math.PI);

                    heightMap[x, y] = Math.Min(
                        (int) (_noise.GetSimplex((float) nx, (float) ny, (float) nz, (float) nw) * 10) +
                        16, 255);
                }
            }

            return heightMap;
        }
    }
}