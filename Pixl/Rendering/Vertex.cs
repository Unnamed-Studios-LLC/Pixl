using System.Runtime.InteropServices;

namespace Pixl;

[StructLayout(LayoutKind.Explicit)]
internal struct Vertex
{
    [FieldOffset(0)]
    public Vec3 Position;
    [FieldOffset(12)]
    public Color Color;

    public Vertex(Vec3 position, Color color)
    {
        Position = position;
        Color = color;
    }
}
