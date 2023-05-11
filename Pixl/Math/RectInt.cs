using System;

namespace Pixl
{
    public struct RectInt
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public RectInt(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Int2 Max => new Int2(Math.Max(X, X + Width), Math.Max(Y, Y + Height));
        public Int2 Min => new Int2(Math.Min(X, X + Width), Math.Min(Y, Y + Height));
        public Int2 Position
        {
            get => new Int2(X, Y);
            set => (X, Y) = value;
        }
        public Int2 Size => new Int2(Width, Height);
    }
}
