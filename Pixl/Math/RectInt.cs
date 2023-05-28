using System;
using System.Diagnostics.CodeAnalysis;

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

        public static bool operator ==(in RectInt a, in RectInt b) => a.Equals(in b);
        public static bool operator !=(in RectInt a, in RectInt b) => !a.Equals(in b);

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is RectInt rectInt) return Equals(in rectInt);
            return base.Equals(obj);
        }
        public bool Equals(in RectInt other) => other.X == X && other.Y == Y && other.Width == Width && other.Height == Height;

        public override int GetHashCode() => base.GetHashCode();
    }
}
