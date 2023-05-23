using System.Runtime.InteropServices;

namespace Pixl;

[StructLayout(LayoutKind.Explicit)]
internal struct PositionTexColorVertex
{
    [FieldOffset(0)]
    public Vec3 Position;
    [FieldOffset(12)]
    public Vec2 TexCoord;
    [FieldOffset(20)]
    public Color32 Color;

    public PositionTexColorVertex(Vec3 position, Vec2 texCoord, Color32 color)
    {
        Position = position;
        TexCoord = texCoord;
        Color = color;
    }
}