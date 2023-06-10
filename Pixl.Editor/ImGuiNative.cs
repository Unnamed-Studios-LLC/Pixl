using ImGuiNET;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Pixl.Editor;

internal unsafe static class ImGuiInternal
{
    public static uint DockBuilderAddNode(uint id) => ImGuiNative.igDockBuilderAddNode(id, ImGuiDockNodeFlags.None);
    public static uint DockBuilderAddNode(uint id, ImGuiDockNodeFlags flags) => ImGuiNative.igDockBuilderAddNode(id, flags);

    public static void DockBuilderDockWindow(string windowName, uint id)
    {
        int num = 0;
        Span<byte> span;
        if (windowName != null)
        {
            num = Encoding.UTF8.GetByteCount(windowName);
            span = num >= 2047 ? Util2.Allocate(num + 1) : stackalloc byte[(int)(uint)(num + 1)];
            int utf = Util2.GetUtf8(windowName, span);
            span[utf] = 0;
        }
        else
        {
            span = null;
        }

        fixed (byte* ptr = span)
        {
            ImGuiNative.igDockBuilderDockWindow(ptr, id);
            if (num >= 2047)
            {
                Util2.Free(ptr);
            }
        }
    }

    public static void DockBuilderFinish(uint id) => ImGuiNative.igDockBuilderFinish(id);

    public static void DockBuilderSetNodePos(uint id, Vector2 position) => ImGuiNative.igDockBuilderSetNodePos(id, position);

    public static void DockBuilderSetNodeSize(uint id, Vector2 size) => ImGuiNative.igDockBuilderSetNodeSize(id, size);

    public static uint DockBuilderSplitNode(uint id, ImGuiDir direction, float multiplier, ref uint outNodeId, ref uint inNodeId) =>
        ImGuiNative.igDockBuilderSplitNode(id, direction, multiplier, (uint*)Unsafe.AsPointer(ref outNodeId), (uint*)Unsafe.AsPointer(ref inNodeId));

    public static void DockBuilderRemoveNode(uint id) => ImGuiNative.igDockBuilderRemoveNode(id);
}

file unsafe static class ImGuiNative
{
    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint igDockBuilderAddNode(uint id, ImGuiDockNodeFlags flags);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern void igDockBuilderDockWindow(byte* windowName, uint id);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern void igDockBuilderFinish(uint id);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern void igDockBuilderSetNodePos(uint id, Vector2 position);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern void igDockBuilderSetNodeSize(uint id, Vector2 size);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint igDockBuilderSplitNode(uint id, ImGuiDir direction, float multiplier, uint* outNodeId, uint* inNodeId);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern void igDockBuilderRemoveNode(uint id);
}

file static class Util2
{
    internal unsafe static Span<byte> Allocate(int byteCount)
    {
        var ptr = (byte*)(void*)Marshal.AllocHGlobal(byteCount);
        return new Span<byte>(ptr, byteCount);
    }

    internal unsafe static void Free(byte* ptr)
    {
        Marshal.FreeHGlobal((IntPtr)ptr);
    }

    internal unsafe static int GetUtf8(string s, Span<byte> utf8Bytes)
    {
        return Encoding.UTF8.GetBytes(s, utf8Bytes);
    }
}
