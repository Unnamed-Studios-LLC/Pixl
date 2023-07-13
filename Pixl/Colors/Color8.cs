using System.Runtime.InteropServices;

namespace Pixl;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct Color8
{
    public byte R;

    public Color8(byte r)
    {
        R = r;
    }

    public static Color8 Black => new(0);
    public static Color8 White => new(255);
}
