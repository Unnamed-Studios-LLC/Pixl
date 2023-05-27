using System;
using CoreGraphics;

namespace Pixl.iOS;

public static class Extensions
{
    public static Int2 ToInt2(this in CGPoint point) => new Int2((int)Math.Floor(point.X), (int)Math.Floor(point.Y));
    public static Int2 ToInt2(this in CGSize size) => new Int2((int)Math.Floor(size.Width), (int)Math.Floor(size.Height));
}

