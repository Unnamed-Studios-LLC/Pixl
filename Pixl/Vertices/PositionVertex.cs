﻿using System.Runtime.InteropServices;

namespace Pixl;

[StructLayout(LayoutKind.Explicit)]
public struct PositionVertex : IVertex
{
    [FieldOffset(0)]
    public Vec3 Position;

    public PositionVertex(Vec3 position)
    {
        Position = position;
    }
}
