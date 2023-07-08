using ImGuiNET;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Pixl.Editor;

internal unsafe sealed class Int8Inspector : RangeInspector<sbyte, sbyte>
{
    public Int8Inspector(FieldInfo field) : base(field)
    {
    }

    protected override RangeAttribute<sbyte> DefaultRange => new(sbyte.MinValue, sbyte.MaxValue, 1);

    protected override void OnSubmitUI(Editor editor, string label, ref sbyte value)
    {
        var min = Range.Min;
        var max = Range.Max;
        ImGui.DragScalar(label, ImGuiDataType.S8, (nint)Unsafe.AsPointer(ref value), Range.Speed, (nint)(&min), (nint)(&max));
    }
}

internal sealed class Int16Inspector : ObjectInspector<short>
{
    protected override void OnSubmitUI(Editor editor, string label, ref short value)
    {
        int intValue = value;
        ImGui.DragInt(label, ref intValue, 1, short.MinValue, short.MaxValue);
        value = (short)intValue;
    }
}

internal sealed class Int32Inspector : ObjectInspector<int>
{
    protected override void OnSubmitUI(Editor editor, string label, ref int value)
    {
        ImGui.DragInt(label, ref value);
    }
}

internal unsafe sealed class Int64Inspector : ObjectInspector<long>
{
    protected override void OnSubmitUI(Editor editor, string label, ref long value)
    {
        ImGui.DragScalar(label, ImGuiDataType.S64, (nint)Unsafe.AsPointer(ref value));
    }
}
internal unsafe sealed class UInt8Inspector : ObjectInspector<byte>
{
    protected override void OnSubmitUI(Editor editor, string label, ref byte value)
    {
        ImGui.DragScalar(label, ImGuiDataType.U8, (nint)Unsafe.AsPointer(ref value));
    }
}

internal sealed class UInt16Inspector : ObjectInspector<ushort>
{
    protected override void OnSubmitUI(Editor editor, string label, ref ushort value)
    {
        int intValue = value;
        ImGui.DragInt(label, ref intValue, 1, ushort.MinValue, ushort.MaxValue);
        value = (ushort)intValue;
    }
}

internal unsafe sealed class UInt32Inspector : ObjectInspector<uint>
{
    protected override void OnSubmitUI(Editor editor, string label, ref uint value)
    {
        long min = uint.MinValue;
        long max = uint.MaxValue;

        long longValue = value;
        ImGui.DragScalar(label, ImGuiDataType.S64, (nint)Unsafe.AsPointer(ref longValue), 1, (nint)Unsafe.AsPointer(ref min), (nint)Unsafe.AsPointer(ref max));
        value = (uint)longValue;
    }
}

internal unsafe sealed class UInt64Inspector : ObjectInspector<ulong>
{
    protected override void OnSubmitUI(Editor editor, string label, ref ulong value)
    {
        ImGui.DragScalar(label, ImGuiDataType.U64, (nint)Unsafe.AsPointer(ref value));
    }
}

internal sealed class FloatInspector : ObjectInspector<float>
{
    protected override void OnSubmitUI(Editor editor, string label, ref float value)
    {
        ImGui.DragFloat(label, ref value);
    }
}

internal unsafe sealed class DoubleInspector : ObjectInspector<double>
{
    protected override void OnSubmitUI(Editor editor, string label, ref double value)
    {
        ImGui.DragScalar(label, ImGuiDataType.Double, (nint)Unsafe.AsPointer(ref value));
    }
}

internal unsafe sealed class BoolInspector : ObjectInspector<bool>
{
    protected override void OnSubmitUI(Editor editor, string label, ref bool value)
    {
        ImGui.Checkbox(label, ref value);
    }
}