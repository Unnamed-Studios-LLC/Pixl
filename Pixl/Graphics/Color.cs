using System.Runtime.InteropServices;

namespace Pixl
{
    public struct Color
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Color(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static Color Black = new Color(0, 0, 0, 255);
        public static Color White = new Color(255, 255, 255, 255);
        public static Color Red = new Color(255, 0, 0, 255);
        public static Color Blue = new Color(0, 0, 255, 255);
        public static Color Green = new Color(0, 255, 0, 255);
        public static Color Yellow = new Color(255, 255, 0, 255);
    }
}
