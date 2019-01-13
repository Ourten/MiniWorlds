using UnityEngine;

namespace GameCode.Scripts.Utils.World
{
    public class Chunk
    {
        public const int SIZE = 16;

        private short[,,] data;
        private readonly World _world;
        public int ChunkX { get; }
        public int ChunkY { get; }
        public int ChunkZ { get; }

        public Chunk(World world, int chunkX, int chunkY, int chunkZ)
        {
            data = new short[SIZE, SIZE, SIZE];

            _world = world;
            ChunkX = chunkX;
            ChunkY = chunkY;
            ChunkZ = chunkZ;
        }

        public void SetBlock(int x, int y, int z, short value)
        {
            data[x, y, z] = value;
        }

        public void SetArea(int xFrom, int yFrom, int zFrom, int xTo, int yTo, int zTo, short value)
        {
            for (var x = xFrom; x < xTo; x++)
            {
                for (var y = yFrom; y < yTo; y++)
                {
                    for (var z = zFrom; z < zTo; z++)
                        SetBlock(x, y, z, value);
                }
            }
        }

        public short GetBlock(int x, int y, int z)
        {
            return data[x, y, z];
        }

        public short GetBlock(Vector3Int pos)
        {
            return data[pos.x, pos.y, pos.z];
        }

        private static readonly Color32[] colors =
            {new Color32(0, 0, 0, 0), new Color32(120, 120, 120, 0), new Color32(51, 153, 51, 0)};

        public Color32 GetBlockColor(short blockType)
        {
            return colors[blockType];
        }
    }
}