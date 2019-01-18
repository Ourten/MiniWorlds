using System;
using System.Collections.Generic;
using System.Linq;

namespace GameCode.Scripts.Utils.World
{
    public static class ChunkGenerator
    {
        private static FastNoise _noise = new FastNoise();

        static ChunkGenerator()
        {
            _noise.SetNoiseType(FastNoise.NoiseType.Perlin);
        }

        public static void Generate(Planet planet, GenerationState state)
        {
            state.InitStep("Heightmap Generation", (int) Math.Pow((double) planet.Size / Chunk.SIZE, 2));

            var heightMap = CreatePlanetHeightMap(planet.Size, planet.Seed, state);
            planet.HeightMap = heightMap;
            
            state.CloseStep();
            state.InitStep("Block Filling", (int) Math.Pow((double) planet.Size / Chunk.SIZE, 2));

            Enumerable.Range(0, (int) Math.Pow((double) planet.Size / Chunk.SIZE, 2)).AsParallel().AsOrdered().ForAll(
                chunkIndex =>
                {
                    var chunkX = chunkIndex % (planet.Size / Chunk.SIZE) * Chunk.SIZE;
                    var chunkY = chunkIndex / (planet.Size / Chunk.SIZE) * Chunk.SIZE;

                    for (var x = 0; x < Chunk.SIZE; x++)
                    {
                        for (var z = 0; z < Chunk.SIZE; z++)
                        {
                            var posX = x + chunkX;
                            var posZ = z + chunkY;
                            planet.SetArea(posX, 0, posZ, posX, heightMap[posX, posZ] - 1, posZ, 1);
                            planet.SetBlock(posX, heightMap[posX, posZ], posZ, 2);
                        }
                    }

                    state.UpdateStep(1);
                });
            state.CloseStep();
        }

        private static int[,] CreatePlanetHeightMap(int size, int seed, GenerationState state)
        {
            var planetHeightMap = JoinHeightMaps(
                Enumerable.Range(0, (int) Math.Pow((double) size / Chunk.SIZE, 2)).AsParallel().AsOrdered()
                    .Select(chunkIndex =>
                    {
                        var mapPart = GenMap(size, chunkIndex, Chunk.SIZE, seed);
                        state.UpdateStep(1);
                        return mapPart;
                    })
                    .ToList(), size);

            return planetHeightMap;
        }

        private static int[,] JoinHeightMaps(IReadOnlyList<int[,]> maps, int size)
        {
            var heightMap = new int[size, size];

            for (var index = 0; index < maps.Count; index++)
            {
                var chunkX = index % (size / Chunk.SIZE) * Chunk.SIZE;
                var chunkY = index / (size / Chunk.SIZE) * Chunk.SIZE;

                for (var x = 0; x < Chunk.SIZE; x++)
                {
                    for (var y = 0; y < Chunk.SIZE; y++)
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