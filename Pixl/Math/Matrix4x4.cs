using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Pixl
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Matrix4x4
    {
        private static readonly Vector128<float> s_adjSignMask = Vector128.Create(1f, -1f, -1f, 1f);

        public static readonly Matrix4x4 Identity = new(
            new Vec4(1, 0, 0, 0),
            new Vec4(0, 1, 0, 0),
            new Vec4(0, 0, 1, 0),
            new Vec4(0, 0, 0, 1)

        );
        
        public static readonly Matrix4x4 Zero = new(
            new Vec4(0, 0, 0, 0),
            new Vec4(0, 0, 0, 0),
            new Vec4(0, 0, 0, 0),
            new Vec4(0, 0, 0, 0)
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

        [FieldOffset(0)]
        internal Vector128<float> Vec0;
        [FieldOffset(16)]
        internal Vector128<float> Vec1;
        [FieldOffset(32)]
        internal Vector128<float> Vec2;
        [FieldOffset(48)]
        internal Vector128<float> Vec3;

        public Matrix4x4(in Vec4 r0, in Vec4 r1, in Vec4 r2, in Vec4 r3) : this()
        {
            C0 = new Vec4(r0.X, r1.X, r2.X, r3.X);
            C1 = new Vec4(r0.Y, r1.Y, r2.Y, r3.Y);
            C2 = new Vec4(r0.Z, r1.Z, r2.Z, r3.Z);
            C3 = new Vec4(r0.W, r1.W, r2.W, r3.W);
        }

        public float this[int row, int column]
        {
            get
            {
                if (row < 0 || row >= 4) throw new ArgumentException("Index cannot be less than 0 or greater than 3", nameof(row));
                if (column < 0 || column >= 4) throw new ArgumentException("Index cannot be less than 0 or greater than 3", nameof(column));
                var address = Unsafe.AsPointer(ref this);
                return *((float*)address + column * 4 + row);
            }
        }

        public float Determinant => GetDeterminant()[0];
        public Matrix4x4 Inverse => GetInverse();
        public Matrix4x4 TransformInverse => GetTransformInverse();

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
            var result = m.Vec0 * v.X +
                m.Vec1 * v.Y +
                m.Vec2 * v.Z +
                m.Vec3 * v.W;
            return *(Vec4*)&result;
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
            new Vec4(2f / (right - left), 0, 0, (left + right) / (left - right)),
            new Vec4(0, 2f / (top - bottom), 0, (top + bottom) / (bottom - top)),
            new Vec4(0, 0, 1.0f / (far - near), near / (near - far)),
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

        /*
        public static Matrix4x4 Rotation(float xSin, float xCos, float ySin, float yCos, float zSin, float zCos) => new Matrix4x4(
            new Vec4(zCos * yCos, zCos * ySin * xSin, 0, 0),
            new Vec4(0, cos, -sin, 0),
            new Vec4(0, sin, cos, 0),
            new Vec4(0, 0, 0, 1)
        );
        */

        public static Matrix4x4 RotationX(float sin, float cos) => new Matrix4x4(
            new Vec4(1, 0, 0, 0),
            new Vec4(0, cos, -sin, 0),
            new Vec4(0, sin, cos, 0),
            new Vec4(0, 0, 0, 1)
        );

        public static Matrix4x4 RotationY(float sin, float cos) => new Matrix4x4(
            new Vec4(cos, 0, sin, 0),
            new Vec4(0, 1, 0, 0),
            new Vec4(-sin, 0, cos, 0),
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


        public static Matrix4x4 Transformation(in Vec3 translation, in Vec3 rotation, in Vec3 scale)
        {
            Transformation(in translation, in rotation, in scale, out var matrix);
            return matrix;
        }

        public static void Transformation(in Vec3 translation, in Vec3 rotation, in Vec3 scale, out Matrix4x4 matrix)
        {
            matrix = Translate(in translation);
            if (rotation.X != 0)
            {
                matrix *= RotationX(MathF.Sin(rotation.X * Angle.Deg2Rad), MathF.Cos(rotation.X * Angle.Deg2Rad));
            }
            if (rotation.Y != 0)
            {
                matrix *= RotationY(MathF.Sin(rotation.Y * Angle.Deg2Rad), MathF.Cos(rotation.Y * Angle.Deg2Rad));
            }
            if (rotation.Z != 0)
            {
                matrix *= RotationZ(MathF.Sin(rotation.Z * Angle.Deg2Rad), MathF.Cos(rotation.Z * Angle.Deg2Rad));
            }
            matrix *= Scale(in scale);
        }

        public static Matrix4x4 View(in Vec3 translation, in Vec3 rotation, in Vec3 scale)
        {
            View(in translation, in rotation, in scale, out var matrix);
            return matrix;
        }

        public static void View(in Vec3 translation, in Vec3 rotation, in Vec3 scale, out Matrix4x4 matrix)
        {
            Transformation(in translation, in rotation, in scale, out matrix);
            matrix = matrix.Inverse;
        }

        public Vec3 MultiplyVector(in Vec2 vector) => MultiplyVector(new Vec3(vector.X, vector.Y, 1));
        public Vec3 MultiplyVector(in Vec3 v)
        {
            return new Vec3(
                M00 * v.X + M01 * v.Y + M02 * v.Z,
                M10 * v.X + M11 * v.Y + M12 * v.Z,
                M20 * v.X + M21 * v.Y + M22 * v.Z
            );
        }

        private Vector128<float> GetDeterminant()
        {
            var a = VecShuffle_0101(in Vec0, in Vec1);
            var c = VecShuffle_2323(in Vec0, in Vec1);
            var b = VecShuffle_0101(in Vec2, in Vec3);
            var d = VecShuffle_2323(in Vec2, in Vec3);

            var detSub = VecShuffle(in Vec0, in Vec2, 0, 2, 0, 2) * VecShuffle(in Vec1, in Vec3, 1, 3, 1, 3) -
                VecShuffle(in Vec0, in Vec2, 1, 3, 1, 3) * VecShuffle(in Vec1, in Vec3, 0, 2, 0, 2);

            var detA = VecSwizzle1(in detSub, 0);
            var detD = VecSwizzle1(in detSub, 3);

            var dC = Mat2AdjMul(in d, in c);
            var aB = Mat2AdjMul(in a, in b);

            var detM = detA * detD;

            var tr = aB * VecSwizzle(dC, 0, 2, 1, 3);
            if (Sse3.IsSupported)
            {
                tr = Sse3.HorizontalAdd(tr, tr);
                tr = Sse3.HorizontalAdd(tr, tr);
            }
            else
            {
                var sum = Vector128.Sum(tr);
                tr = Vector128.Create(sum);
            }

            detM -= tr;
            return detM;
        }

        private Matrix4x4 GetInverse()
        {
            var a = VecShuffle_0101(in Vec0, in Vec1);
            var c = VecShuffle_2323(in Vec0, in Vec1);
            var b = VecShuffle_0101(in Vec2, in Vec3);
            var d = VecShuffle_2323(in Vec2, in Vec3);

            var detSub = VecShuffle(in Vec0, in Vec2, 0, 2, 0, 2) * VecShuffle(in Vec1, in Vec3, 1, 3, 1, 3) -
                VecShuffle(in Vec0, in Vec2, 1, 3, 1, 3) * VecShuffle(in Vec1, in Vec3, 0, 2, 0, 2);

            var detA = VecSwizzle1(in detSub, 0);
            var detC = VecSwizzle1(in detSub, 1);
            var detB = VecSwizzle1(in detSub, 2);
            var detD = VecSwizzle1(in detSub, 3);

            var dC = Mat2AdjMul(in d, in c);
            var aB = Mat2AdjMul(in a, in b);

            var detM = detA * detD;
            var tr = aB * VecSwizzle(dC, 0, 2, 1, 3);
            if (Sse3.IsSupported)
            {
                tr = Sse3.HorizontalAdd(tr, tr);
                tr = Sse3.HorizontalAdd(tr, tr);
            }
            else
            {
                var sum = Vector128.Sum(tr);
                tr = Vector128.Create(sum);
            }

            detM -= tr;
            if (detM[0] == 0)
            {
                return Zero;
            }

            var rDetM = s_adjSignMask / detM;
            var x = (detD * a - Mat2Mul(in b, in dC)) * rDetM;
            var y = (detB * c - Mat2MulAdj(in d, in aB)) * rDetM;
            var z = (detC * b - Mat2MulAdj(in a, in dC)) * rDetM;
            var w = (detA * d - Mat2Mul(in c, in aB)) * rDetM;

            Matrix4x4 result = default;
            result.Vec0 = VecShuffle(x, z, 3, 1, 3, 1);
            result.Vec1 = VecShuffle(x, z, 2, 0, 2, 0);
            result.Vec2 = VecShuffle(y, w, 3, 1, 3, 1);
            result.Vec3 = VecShuffle(y, w, 2, 0, 2, 0);
            return result;
        }


        private Matrix4x4 GetTransformInverse()
        {
            const float smallNumber = 1e-8f;
            Matrix4x4 r = default;

            var t0 = VecShuffle_0101(in Vec0, in Vec1);
            var t1 = VecShuffle_2323(in Vec0, in Vec1);

            r.Vec0 = VecShuffle(in t0, in Vec2, 0, 2, 0, 3);
            r.Vec1 = VecShuffle(in t0, in Vec2, 1, 3, 1, 3);
            r.Vec2 = VecShuffle(in t1, in Vec2, 0, 2, 2, 3);

            var sizeSqr = r.Vec0 * r.Vec0 + r.Vec1 * r.Vec1 + r.Vec2 * r.Vec2;
            var one = Vector128.Create(1f);
            var rSizeSqr = Vector128.ConditionalSelect(
                Vector128.LessThan(sizeSqr, Vector128.Create(smallNumber)),
                one,
                one / sizeSqr
            );

            r.Vec0 *= rSizeSqr;
            r.Vec1 *= rSizeSqr;
            r.Vec2 *= rSizeSqr;

            r.Vec3 = r.Vec0 * VecSwizzle1(in Vec3, 0) + r.Vec1 * VecSwizzle1(in Vec3, 1) + r.Vec2 * VecSwizzle1(in Vec3, 2);
            r.Vec3 = Vector128.Create(0f, 0f, 0f, 1f) - r.Vec3;

            return r;
        }

        private static Vector128<float> Mat2Mul(in Vector128<float> vec1, in Vector128<float> vec2)
        {
            return vec1 * VecSwizzle(in vec2, 0, 0, 3, 3) +
                VecSwizzle(in vec1, 2, 3, 0, 1) * VecSwizzle(in vec2, 1, 1, 2, 2);
        }

        private static Vector128<float> Mat2AdjMul(in Vector128<float> vec1, in Vector128<float> vec2)
        {
            return VecSwizzle(in vec1, 3, 0, 3, 0) * vec2 -
                VecSwizzle(in vec1, 2, 1, 2, 1) * VecSwizzle(in vec2, 1, 0, 3, 2);
        }

        private static Vector128<float> Mat2MulAdj(in Vector128<float> vec1, in Vector128<float> vec2)
        {
            return vec1 * VecSwizzle(in vec2, 3, 3, 0, 0) -
                VecSwizzle(in vec1, 2, 3, 0, 1) * VecSwizzle(in vec2, 1, 1, 2, 2);
        }

        private static Vector128<float> VecSwizzle(in Vector128<float> vec, int x, int y, int z, int w) => Vector128.Shuffle(vec, Vector128.Create(x, y, z, w));
        private static Vector128<float> VecSwizzle(in Vector128<float> vec, Vector128<int> mask) => Vector128.Shuffle(vec, mask);
        private static Vector128<float> VecSwizzle1(in Vector128<float> vec, int mask) => VecSwizzle(in vec, Vector128.Create(mask));
        private static Vector128<float> VecSwizzle_0022(in Vector128<float> vec) => Vector128.Create(vec[0], vec[0], vec[2], vec[2]);
        private static Vector128<float> VecSwizzle_1133(in Vector128<float> vec) => Vector128.Create(vec[1], vec[1], vec[3], vec[3]);

        private static Vector128<float> VecShuffle(in Vector128<float> vec1, in Vector128<float> vec2, int x, int y, int z, int w) => Vector128.Create(vec1[x], vec1[y], vec2[z], vec2[w]);
        private static Vector128<float> VecShuffle_0101(in Vector128<float> vec1, in Vector128<float> vec2) => Vector128.Create(vec1[0], vec1[1], vec2[0], vec2[1]);
        private static Vector128<float> VecShuffle_2323(in Vector128<float> vec1, in Vector128<float> vec2) => Vector128.Create(vec1[2], vec1[3], vec2[2], vec2[3]);
    }
}
