using System;
using System.Runtime.InteropServices;

namespace Pixl
{
    [Serializable]
    [Inline]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vec2
    {
        public float X;
        public float Y;

        public Vec2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vec2 Half = new Vec2(0.5f, 0.5f);
        public static Vec2 One = new Vec2(1, 1);
        public static Vec2 Zero = new Vec2(0, 0);

        public static Vec2 operator +(in Vec2 a, float v) => new Vec2(a.X + v, a.Y + v);
        public static Vec2 operator -(in Vec2 a, float v) => new Vec2(a.X - v, a.Y - v);
        public static Vec2 operator *(in Vec2 a, float v) => new Vec2(a.X * v, a.Y * v);
        public static Vec2 operator /(in Vec2 a, float v) => new Vec2(a.X / v, a.Y / v);

        public static Vec2 operator +(in Vec2 a, in Vec2 b) => new Vec2(a.X + b.X, a.Y + b.Y);
        public static Vec2 operator -(in Vec2 a, in Vec2 b) => new Vec2(a.X - b.X, a.Y - b.Y);
        public static Vec2 operator *(in Vec2 a, in Vec2 b) => new Vec2(a.X * b.X, a.Y * b.Y);
        public static Vec2 operator /(in Vec2 a, in Vec2 b) => new Vec2(a.X / b.X, a.Y / b.Y);

        public static implicit operator Vec2(in float value) => new Vec2(value, value);
        public static implicit operator Vec2(in Int2 int2) => new Vec2(int2.X, int2.Y);
        public static explicit operator Vec2(in Int3 int3) => new Vec2(int3.X, int3.Y);
        public static explicit operator Vec2(in Vec3 vec3) => new Vec2(vec3.X, vec3.Y);
        public static explicit operator Vec2(in Int4 int4) => new Vec2(int4.X, int4.Y);
        public static explicit operator Vec2(in Vec4 vec4) => new Vec2(vec4.X, vec4.Y);

        public static Vec2 Max(in Vec2 a, in Vec2 b) => new Vec2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        public static Vec2 Min(in Vec2 a, in Vec2 b) => new Vec2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));

        public void Deconstruct(out float x, out float y) { x = X; y = Y; }
    }
}
