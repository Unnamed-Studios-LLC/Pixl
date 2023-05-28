using System.Runtime.InteropServices;

namespace Pixl.Editor;

[StructLayout(LayoutKind.Explicit)]
internal struct GuiVertex
{
    [FieldOffset(0)]
    public Vec2 Position;
    [FieldOffset(8)]
    public Vec2 TexCoord;
    [FieldOffset(16)]
    public Color32 Color;
}
