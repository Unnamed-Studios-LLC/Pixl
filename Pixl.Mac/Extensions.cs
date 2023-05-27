using System;
using System.Drawing;
using AppKit;
using CoreGraphics;

namespace Pixl.Mac;

public static class Extensions
{
    public static NSWindowStyle GetWindowStyles(this WindowStyle windowStyle) => windowStyle switch
    {
        _ => NSWindowStyle.Closable | NSWindowStyle.Titled | NSWindowStyle.Resizable | NSWindowStyle.Miniaturizable
    };

    public static Int2 ToInt2(this in CGPoint point) => new Int2((int)Math.Floor(point.X), (int)Math.Floor(point.Y));
    public static Int2 ToInt2(this in CGSize size) => new Int2((int)Math.Floor(size.Width), (int)Math.Floor(size.Height));
}

