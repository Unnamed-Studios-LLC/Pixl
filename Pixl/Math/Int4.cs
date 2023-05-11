namespace Pixl
{
    public struct Int4
    {
        public int X;
        public int Y;
        public int Z;
        public int W;

        public Int4(int x, int y, int z, int w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Int2 Xy => new Int2(X, Y);
        public Int3 Xyz => new Int3(X, Y, Z);

        public static implicit operator Int4(in int value) => new Int4(value, value, value, value);
        public static implicit operator Int4(in Int2 int2) => new Int4(int2.X, int2.Y, default, default);
        public static explicit operator Int4(in Vec2 vec2) => new Int4((int)MathF.Floor(vec2.X), (int)MathF.Floor(vec2.Y), default, default);
        public static implicit operator Int4(in Int3 int3) => new Int4(int3.X, int3.Y, int3.Z, default);
        public static explicit operator Int4(in Vec3 vec3) => new Int4((int)MathF.Floor(vec3.X), (int)MathF.Floor(vec3.Y), (int)MathF.Floor(vec3.Z), default);
        public static explicit operator Int4(in Vec4 vec4) => new Int4((int)MathF.Floor(vec4.X), (int)MathF.Floor(vec4.Y), (int)MathF.Floor(vec4.Z), (int)MathF.Floor(vec4.W));

        public void Deconstruct(out int x, out int y) { x = X; y = Y; }
        public void Deconstruct(out int x, out int y, out int z) { x = X; y = Y; z = Z; }
        public void Deconstruct(out int x, out int y, out int z, out int w) { x = X; y = Y; z = Z; w = W; }
    }
}
