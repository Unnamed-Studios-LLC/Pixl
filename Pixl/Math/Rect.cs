namespace Pixl
{
    public struct Rect
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public Rect(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Vec2 Max => new Vec2(Math.Max(X, X + Width), Math.Max(Y, Y + Height));
        public Vec2 Min => new Vec2(Math.Min(X, X + Width), Math.Min(Y, Y + Height));
        public Vec2 Position => new Vec2(X, Y);
        public Vec2 Size => new Vec2(Width, Height);
    }
}
