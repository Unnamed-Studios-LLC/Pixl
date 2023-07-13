using System.Runtime.InteropServices;

namespace Pixl
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vec4
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public Vec4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Vec2 Xy => new Vec2(X, Y);
        public Vec3 Xyz => new Vec3(X, Y, Z);

        public static Vec4 operator +(in Vec4 a, in float v) => new Vec4(a.X + v, a.Y + v, a.Z + v, a.W + v);
        public static Vec4 operator -(in Vec4 a, in float v) => new Vec4(a.X - v, a.Y - v, a.Z - v, a.W - v);
        public static Vec4 operator *(in Vec4 a, in float v) => new Vec4(a.X * v, a.Y * v, a.Z * v, a.W * v);
        public static Vec4 operator /(in Vec4 a, in float v) => new Vec4(a.X / v, a.Y / v, a.Z / v, a.W / v);

        public static Vec4 operator +(in Vec4 a, in Vec4 b) => new Vec4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        public static Vec4 operator -(in Vec4 a, in Vec4 b) => new Vec4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
        public static Vec4 operator *(in Vec4 a, in Vec4 b) => new Vec4(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
        public static Vec4 operator /(in Vec4 a, in Vec4 b) => new Vec4(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.W / b.W);

        public static implicit operator Vec4(in float value) => new Vec4(value, value, value, value);
        public static implicit operator Vec4(in Int2 int2) => new Vec4(int2.X, int2.Y, default, default);
        public static implicit operator Vec4(in Vec2 vec2) => new Vec4(vec2.X, vec2.Y, default, default);
        public static implicit operator Vec4(in Int3 int3) => new Vec4(int3.X, int3.Y, int3.Z, default);
        public static implicit operator Vec4(in Vec3 vec3) => new Vec4(vec3.X, vec3.Y, vec3.Z, default);
        public static implicit operator Vec4(in Int4 int4) => new Vec4(int4.X, int4.Y, int4.Z, int4.W);

        public static Vec4 Max(in Vec4 a, in Vec4 b) => new Vec4(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z), Math.Max(a.W, b.W));
        public static Vec4 Min(in Vec4 a, in Vec4 b) => new Vec4(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z), Math.Min(a.W, b.W));

        public void Deconstruct(out float x, out float y) { x = X; y = Y; }
        public void Deconstruct(out float x, out float y, out float z) { x = X; y = Y; z = Z; }
        public void Deconstruct(out float x, out float y, out float z, out float w) { x = X; y = Y; z = Z; w = W; }
    }
}
