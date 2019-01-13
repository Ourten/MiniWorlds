namespace GameCode.Utils.Mesh
{
    public struct Quad
    {
        private readonly float _x;
        private readonly float _y;
        private readonly float _w;
        private readonly float _h;

        public Quad(float x, float y, float w, float h)
        {
            _x = x;
            _y = y;
            _w = w;
            _h = h;
        }

        public float X => _x;

        public float Y => _y;

        public float W => _w;

        public float H => _h;
    }
}