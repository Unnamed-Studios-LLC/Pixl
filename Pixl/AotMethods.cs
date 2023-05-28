using System.Runtime.InteropServices;

namespace Pixl;

internal static class AotMethods
{
    /// <summary>
    /// Method is used for AOT generation only, not meant to be called by runtime
    /// </summary>
    internal static void Aot()
    {
        var size = Marshal.SizeOf<PositionTexColorVertex>();
        size = Marshal.SizeOf<PositionColorVertex>();
        size = Marshal.SizeOf<PositionVertex>();
    }
}
