using System.Runtime.InteropServices;

namespace Pixl
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Matrix4x4
    {
        public static readonly Matrix4x4 Identity = new Matrix4x4(
            new Vec4(1, 0, 0, 0),
            new Vec4(0, 1, 0, 0),
            new Vec4(0, 0, 1, 0),
            new Vec4(0, 0, 0, 1)
        );

        [FieldOffset(0)]
        public float M00;
        [FieldOffset(4)]
        public float M10;
        [FieldOffset(8)]
        public float M20;
        [FieldOffset(12)]
        public float M30;
        [FieldOffset(16)]
        public float M01;
        [FieldOffset(20)]
        public float M11;
        [FieldOffset(24)]
        public float M21;
        [FieldOffset(28)]
        public float M31;
        [FieldOffset(32)]
        public float M02;
        [FieldOffset(36)]
        public float M12;
        [FieldOffset(40)]
        public float M22;
        [FieldOffset(44)]
        public float M32;
        [FieldOffset(48)]
        public float M03;
        [FieldOffset(52)]
        public float M13;
        [FieldOffset(56)]
        public float M23;
        [FieldOffset(60)]
        public float M33;

        [FieldOffset(0)]
        public Vec4 C0;
        [FieldOffset(16)]
        public Vec4 C1;
        [FieldOffset(32)]
        public Vec4 C2;
        [FieldOffset(48)]
        public Vec4 C3;

        public Matrix4x4(in Vec4 r0, in Vec4 r1, in Vec4 r2, in Vec4 r3) : this()
        {
            C0 = new Vec4(r0.X, r1.X, r2.X, r3.X);
            C1 = new Vec4(r0.Y, r1.Y, r2.Y, r3.Y);
            C2 = new Vec4(r0.Z, r1.Z, r2.Z, r3.Z);
            C3 = new Vec4(r0.W, r1.W, r2.W, r3.W);
        }

        public Vec4 R0 => new Vec4(M00, M01, M02, M03);
        public Vec4 R1 => new Vec4(M10, M11, M12, M13);
        public Vec4 R2 => new Vec4(M20, M21, M22, M23);
        public Vec4 R3 => new Vec4(M30, M31, M32, M33);

        public static Matrix4x4 operator *(in Matrix4x4 a, in Matrix4x4 b)
        {
            return FromColumns(
                a * b.C0,
                a * b.C1,
                a * b.C2,
                a * b.C3
            );
        }


        public static Vec4 operator *(in Matrix4x4 m, in Vec2 v) => m * new Vec4(v.X, v.Y, 1, 1);
        public static Vec4 operator *(in Matrix4x4 m, in Vec3 v) => m * new Vec4(v.X, v.Y, v.Z, 1);
        public static Vec4 operator *(in Matrix4x4 m, in Vec4 v)
        {
            return new Vec4(
                m.M00 * v.X + m.M01 * v.Y + m.M02 * v.Z + m.M03 * v.W,
                m.M10 * v.X + m.M11 * v.Y + m.M12 * v.Z + m.M13 * v.W,
                m.M20 * v.X + m.M21 * v.Y + m.M22 * v.Z + m.M23 * v.W,
                m.M30 * v.X + m.M31 * v.Y + m.M32 * v.Z + m.M33 * v.W
            );
        }

        public static Vec4 operator *(in Vec2 v, in Matrix4x4 m) => new Vec4(v.X, v.Y, 1, 1) * m;
        public static Vec4 operator *(in Vec3 v, in Matrix4x4 m) => new Vec4(v.X, v.Y, v.Z, 1) * m;
        public static Vec4 operator *(in Vec4 v, in Matrix4x4 m)
        {
            return new Vec4(
                v.X * m.M00 + v.Y * m.M10 + v.Z * m.M20 + v.W * m.M30,
                v.X * m.M01 + v.Y * m.M11 + v.Z * m.M21 + v.W * m.M31,
                v.X * m.M02 + v.Y * m.M12 + v.Z * m.M22 + v.W * m.M32,
                v.X * m.M03 + v.Y * m.M13 + v.Z * m.M23 + v.W * m.M33
            );
        }

        public static Matrix4x4 FromColumns(in Vec4 c0, in Vec4 c1, in Vec4 c2, in Vec4 c3)
        {
            var m = new Matrix4x4
            {
                C0 = c0,
                C1 = c1,
                C2 = c2,
                C3 = c3
            };
            return m;
        }

        public static Matrix4x4 Orthographic(float left, float right, float bottom, float top, float near, float far) => new Matrix4x4(
            new Vec4(2f / (right - left), 0, 0, -(right + left) / (right - left)),
            new Vec4(0, 2f / (top - bottom), 0, -(top + bottom) / (top - bottom)),
            new Vec4(0, 0, -2f / (far - near), -(far + near) / (far - near)),
            new Vec4(0, 0, 0, 1)
        );

        public static Matrix4x4 Shear(float left, float right, float bottom, float top, float near, float far, float cameraZ, float scale, float shearScale)
        {
            var zOffset = cameraZ * scale;
            var s = shearScale / top;
            return new Matrix4x4(
                new Vec4(2f / (right - left), 0, 0, 0),
                new Vec4(0, 2f / (top - bottom), s, zOffset * s),
                new Vec4(0, 0, -2f / (far - near), -(far + near) / (far - near)),
                new Vec4(0, 0, 0, 1)
            );
        }

        public static Matrix4x4 RotationX(float sin, float cos) => new Matrix4x4(
            new Vec4(1, 0, 0, 0),
            new Vec4(0, cos, sin, 0),
            new Vec4(0, -sin, cos, 0),
            new Vec4(0, 0, 0, 1)
        );

        public static Matrix4x4 RotationY(float sin, float cos) => new Matrix4x4(
            new Vec4(cos, 0, -sin, 0),
            new Vec4(0, 1, 0, 0),
            new Vec4(sin, 0, cos, 0),
            new Vec4(0, 0, 0, 1)
        );

        public static Matrix4x4 RotationZ(float sin, float cos) => new Matrix4x4(
            new Vec4(cos, -sin, 0, 0),
            new Vec4(sin, cos, 0, 0),
            new Vec4(0, 0, 1, 0),
            new Vec4(0, 0, 0, 1)
        );

        public static Matrix4x4 Scale(in Vec3 xyz) => new Matrix4x4(
            new Vec4(xyz.X, 0, 0, 0),
            new Vec4(0, xyz.Y, 0, 0),
            new Vec4(0, 0, xyz.Z, 0),
            new Vec4(0, 0, 0, 1)
        );

        public static Matrix4x4 Translate(in Vec3 xyz) => new Matrix4x4(
            new Vec4(1, 0, 0, xyz.X),
            new Vec4(0, 1, 0, xyz.Y),
            new Vec4(0, 0, 1, xyz.Z),
            new Vec4(0, 0, 0, 1)
        );

        public Vec3 MultiplyVector(in Vec2 vector) => MultiplyVector(new Vec3(vector.X, vector.Y, 1));
        public Vec3 MultiplyVector(in Vec3 v)
        {
            return new Vec3(
                M00 * v.X + M01 * v.Y + M02 * v.Z,
                M10 * v.X + M11 * v.Y + M12 * v.Z,
                M20 * v.X + M21 * v.Y + M22 * v.Z
            );
        }
    }
}
