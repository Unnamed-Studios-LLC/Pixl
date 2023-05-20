namespace Pixl
{
    public struct Int2
    {
        public int X;
        public int Y;

        public Int2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Int2 One = new Int2(1, 1);
        public static Int2 Zero = new Int2(0, 0);

        public static bool operator ==(in Int2 a, in Int2 b) => a.Equals(in b);
        public static bool operator !=(in Int2 a, in Int2 b) => !a.Equals(in b);

        public static Int2 operator +(in Int2 a, int v) => new Int2(a.X + v, a.Y + v);
        public static Int2 operator -(in Int2 a, int v) => new Int2(a.X - v, a.Y - v);
        public static Int2 operator *(in Int2 a, int v) => new Int2(a.X * v, a.Y * v);
        public static Int2 operator /(in Int2 a, int v) => new Int2(a.X / v, a.Y / v);
                      
        public static Int2 operator +(in Int2 a, in Int2 b) => new Int2(a.X + b.X, a.Y + b.Y);
        public static Int2 operator -(in Int2 a, in Int2 b) => new Int2(a.X - b.X, a.Y - b.Y);
        public static Int2 operator *(in Int2 a, in Int2 b) => new Int2(a.X * b.X, a.Y * b.Y);
        public static Int2 operator /(in Int2 a, in Int2 b) => new Int2(a.X / b.X, a.Y / b.Y);

        public static implicit operator Int2(in int value) => new Int2(value, value);
        public static explicit operator Int2(in Vec2 vec2) => new Int2((int)MathF.Floor(vec2.X), (int)MathF.Floor(vec2.Y));
        public static explicit operator Int2(in Int3 int3) => new Int2(int3.X, int3.Y);
        public static explicit operator Int2(in Vec3 vec3) => new Int2((int)MathF.Floor(vec3.X), (int)MathF.Floor(vec3.Y));
        public static explicit operator Int2(in Int4 int4) => new Int2(int4.X, int4.Y);
        public static explicit operator Int2(in Vec4 vec4) => new Int2((int)MathF.Floor(vec4.X), (int)MathF.Floor(vec4.Y));

        public static Int2 Max(in Int2 a, in Int2 b) => new Int2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        public static Int2 Min(in Int2 a, in Int2 b) => new Int2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));

        public void Deconstruct(out int x, out int y) { x = X; y = Y; }

        public override bool Equals(object? obj)
        {
            if (obj is Int2 other) return Equals(in other);
            return base.Equals(obj);
        }

        public bool Equals(in Int2 other) => other.X == X && other.Y == Y;

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => $"({X}, {Y})";
    }
}
