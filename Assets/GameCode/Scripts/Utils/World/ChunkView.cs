namespace GameCode.Scripts.Utils.World
{
    public class ChunkView
    {
        public readonly Chunk Chunk;
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public ChunkView(Chunk chunk, int x, int y, int z)
        {
            Chunk = chunk;
            X = x;
            Y = y;
            Z = z;
        }
    }
}