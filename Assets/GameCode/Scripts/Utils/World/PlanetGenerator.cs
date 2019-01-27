using System;
using System.Collections.Generic;
using System.Linq;
using GameCode.Scripts.Utils.World.Biome;

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
            state.InitStep("Biomes Generation", (int) Math.Pow((double) planet.Size / Chunk.SIZE, 2));            
            planet.MoistureMap = CreatePlanetMoistureMap(planet.Size, new Random().Next(), state);
            planet.BiomeMap = new short[planet.Size, planet.Size];

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

                            var biomeID = Biomes.GetBiome(heightMap[posX, posZ], planet.MoistureMap[posX, posZ]).ID;
                            planet.BiomeMap[posX, posZ] = biomeID;

                            planet.SetArea(posX, 0, posZ, posX, heightMap[posX, posZ] - 1, posZ, (short) (biomeID + 1));
                            planet.SetBlock(posX, heightMap[posX, posZ], posZ, (short) (biomeID + 1));
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
                        var mapPart = GenMap(size, chunkIndex, Chunk.SIZE, seed,
                            value => (int) Math.Min(value * 10 + 16, 255));
                        state.UpdateStep(1);
                        return mapPart;
                    })
                    .ToList(), size);

            return planetHeightMap;
        }

        private static float[,] CreatePlanetMoistureMap(int size, int seed, GenerationState state)
        {
            var planetMoistureMap = JoinHeightMaps(
                Enumerable.Range(0, (int) Math.Pow((double) size / Chunk.SIZE, 2)).AsParallel().AsOrdered()
                    .Select(chunkIndex =>
                    {
                        var mapPart = GenMap(size, chunkIndex, Chunk.SIZE, seed,
                            value => (value + 1) /2);
                        state.UpdateStep(1);
                        return mapPart;
                    })
                    .ToList(), size);

            return planetMoistureMap;
        }

        private static T[,] JoinHeightMaps<T>(IReadOnlyList<T[,]> maps, int size)
        {
            var heightMap = new T[size, size];

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

        private static T[,] GenMap<T>(int size, int chunkIndex, int chunkSize, int seed,
            Func<float, T> valueTransformer)
        {
            var heightMap = new T[chunkSize, chunkSize];
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

                    var nx = (float) (x1 + Math.Cos(s * 2 * Math.PI) * dx / (2 * Math.PI));
                    var ny = (float) (y1 + Math.Cos(t * 2 * Math.PI) * dy / (2 * Math.PI));
                    var nz = (float) (x1 + Math.Sin(s * 2 * Math.PI) * dx / (2 * Math.PI));
                    var nw = (float) (y1 + Math.Sin(t * 2 * Math.PI) * dy / (2 * Math.PI));

                    heightMap[x, y] = valueTransformer.Invoke(
                        _noise.GetSimplex(0.25f * nx, 0.25f * ny, 0.25f * nz, 0.25f * nw) /*
                        + 0.5f * _noise.GetSimplex(nx, ny, nz, nw)*/);
                }
            }

            return heightMap;
        }
    }
}