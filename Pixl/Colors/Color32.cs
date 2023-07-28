using System.Runtime.InteropServices;

namespace Pixl;

[Serializable]
[Inline]
[StructLayout(LayoutKind.Sequential)]
public struct Color32
{
    public byte R;
    public byte G;
    public byte B;
    public byte A;

    public Color32(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public static Color32 Black => new(0, 0, 0, 255);
    public static Color32 Blue => new(0, 0, 255, 255);
    public static Color32 Green => new(0, 255, 0, 255);
    public static Color32 Red => new(255, 0, 0, 255);
    public static Color32 Yellow => new(255, 255, 0, 255);
    public static Color32 White => new(255, 255, 255, 255);
}
