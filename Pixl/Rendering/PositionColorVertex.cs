using System.Runtime.InteropServices;

namespace Pixl;

[StructLayout(LayoutKind.Explicit)]
internal struct PositionColorVertex
{
    [FieldOffset(0)]
    public Vec3 Position;
    [FieldOffset(12)]
    public Color Color;

    public PositionColorVertex(Vec3 position, Color color)
    {
        Position = position;
        Color = color;
    }
}
