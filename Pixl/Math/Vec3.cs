using System.Runtime.InteropServices;

namespace Pixl
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vec3
    {
        public float X;
        public float Y;
        public float Z;

        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vec3 Half = new Vec3(0.5f, 0.5f, 0.5f);
        public static Vec3 One = new Vec3(1, 1, 1);
        public static Vec3 Zero = new Vec3(0, 0, 0);

        public Vec2 Xy => new Vec2(X, Y);

        public static Vec3 operator +(in Vec3 a, in float v) => new Vec3(a.X + v, a.Y + v, a.Z + v);
        public static Vec3 operator -(in Vec3 a, in float v) => new Vec3(a.X - v, a.Y - v, a.Z - v);
        public static Vec3 operator *(in Vec3 a, in float v) => new Vec3(a.X * v, a.Y * v, a.Z * v);
        public static Vec3 operator /(in Vec3 a, in float v) => new Vec3(a.X / v, a.Y / v, a.Z / v);

        public static Vec3 operator +(in Vec3 a, in Vec3 b) => new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vec3 operator -(in Vec3 a, in Vec3 b) => new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vec3 operator *(in Vec3 a, in Vec3 b) => new Vec3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        public static Vec3 operator /(in Vec3 a, in Vec3 b) => new Vec3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);

        public static implicit operator Vec3(in float value) => new Vec3(value, value, value);
        public static implicit operator Vec3(in Int2 int2) => new Vec3(int2.X, int2.Y, default);
        public static implicit operator Vec3(in Vec2 vec2) => new Vec3(vec2.X, vec2.Y, default);
        public static implicit operator Vec3(in Int3 int3) => new Vec3(int3.X, int3.Y, int3.Z);
        public static explicit operator Vec3(in Int4 int4) => new Vec3(int4.X, int4.Y, int4.Z);
        public static explicit operator Vec3(in Vec4 vec4) => new Vec3(vec4.X, vec4.Y, vec4.Z);

        public static Vec3 Max(in Vec3 a, in Vec3 b) => new Vec3(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
        public static Vec3 Min(in Vec3 a, in Vec3 b) => new Vec3(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));

        public void Deconstruct(out float x, out float y) { x = X; y = Y; }
        public void Deconstruct(out float x, out float y, out float z) { x = X; y = Y; z = Z; }
    }
}
