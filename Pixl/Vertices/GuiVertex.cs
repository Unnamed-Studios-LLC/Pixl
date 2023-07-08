using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pixl;

[StructLayout(LayoutKind.Explicit)]
internal struct GuiVertex : IVertex
{
    [FieldOffset(0)]
    public Vec2 Position;
    [FieldOffset(8)]
    public Vec2 TexCoord;
    [FieldOffset(16)]
    public Color32 Color;
}