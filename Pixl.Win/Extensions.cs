using NetCoreEx.Geometry;
using WinApi.User32;

namespace Pixl.Win;

public static class Extensions
{
    public static WindowStyles GetWindowStyles(this WindowStyle windowStyle) => windowStyle switch
    {
        _ => WindowStyles.WS_OVERLAPPEDWINDOW
    };

    public static Int2 ToInt2(this in Rectangle rectangle) => new Int2(rectangle.Size.Width, rectangle.Size.Height);

    public static Int2 ToWindowedSize(this Int2 clientSize)
    {
        var rect = new Rectangle(clientSize.X, clientSize.Y);
        if (!User32Methods.AdjustWindowRect(ref rect, WindowStyles.WS_OVERLAPPEDWINDOW, false)) return clientSize;
        return rect.ToInt2();
    }
}
