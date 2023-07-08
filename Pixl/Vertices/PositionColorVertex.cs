using System.Runtime.InteropServices;

namespace Pixl;

[StructLayout(LayoutKind.Explicit)]
internal struct PositionColorVertex : IVertex
{
    [FieldOffset(0)]
    public Vec3 Position;
    [FieldOffset(12)]
    public Color32 Color;

    public PositionColorVertex(Vec3 position, Color32 color)
    {
        Position = position;
        Color = color;
    }
}
