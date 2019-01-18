using UnityEngine;

namespace GameCode.Scripts.Utils.World.Biome
{
    public static class Biomes
    {
        public static readonly Biome PLAIN = new Biome(0, new Color32(108, 229, 78, 255));
        public static readonly Biome FOREST = new Biome(1, new Color32(58, 140, 37, 255));
        public static readonly Biome DESERT = new Biome(2, new Color32(225, 226, 118, 255));
        public static readonly Biome RAIN_FOREST = new Biome(3, new Color32(11, 196, 27, 255));

    }

    public class Biome
    {
        public short ID { get; }
        public Color32 Color { get; }

        public Biome(short id, Color32 color)
        {
            ID = id;
            Color = color;
        }
    }
}