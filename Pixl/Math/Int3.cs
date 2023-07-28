using System.Runtime.InteropServices;

namespace Pixl
{
    [Serializable]
    [Inline]
    [StructLayout(LayoutKind.Sequential)]
    public struct Int3
    {
        public int X;
        public int Y;
        public int Z;

        public Int3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Int2 Xy => new Int2(X, Y);

        public static Int3 operator +(in Int3 a, int v) => new Int3(a.X + v, a.Y + v, a.Z + v);
        public static Int3 operator -(in Int3 a, int v) => new Int3(a.X - v, a.Y - v, a.Z - v);
        public static Int3 operator *(in Int3 a, int v) => new Int3(a.X * v, a.Y * v, a.Z * v);
        public static Int3 operator /(in Int3 a, int v) => new Int3(a.X / v, a.Y / v, a.Z / v);

        public static Int3 operator +(in Int3 a, in Int3 b) => new Int3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Int3 operator -(in Int3 a, in Int3 b) => new Int3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Int3 operator *(in Int3 a, in Int3 b) => new Int3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        public static Int3 operator /(in Int3 a, in Int3 b) => new Int3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);

        public static implicit operator Int3(in int value) => new Int3(value, value, value);
        public static implicit operator Int3(in Int2 int2) => new Int3(int2.X, int2.Y, default);
        public static explicit operator Int3(in Vec2 vec2) => new Int3((int)MathF.Floor(vec2.X), (int)MathF.Floor(vec2.Y), default);
        public static explicit operator Int3(in Vec3 vec3) => new Int3((int)MathF.Floor(vec3.X), (int)MathF.Floor(vec3.Y), (int)MathF.Floor(vec3.Z));
        public static explicit operator Int3(in Int4 int4) => new Int3(int4.X, int4.Y, int4.Z);
        public static explicit operator Int3(in Vec4 vec4) => new Int3((int)MathF.Floor(vec4.X), (int)MathF.Floor(vec4.Y), (int)MathF.Floor(vec4.Z));

        public static Int3 Max(in Int3 a, in Int3 b) => new Int3(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
        public static Int3 Min(in Int3 a, in Int3 b) => new Int3(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));

        public void Deconstruct(out int x, out int y) { x = X; y = Y; }
        public void Deconstruct(out int x, out int y, out int z) { x = X; y = Y; z = Z; }
    }
}
