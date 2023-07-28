using ImGuiNET;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Pixl.Editor;

internal unsafe sealed class Color32Inspector : ObjectInspector<Color32>
{
    protected override void OnSubmitUI(Editor editor, string label, ref Color32 value)
    {
        var vectorColor = new Vector4(value.R / 255f, value.G / 255f, value.B / 255f, value.A / 255f);
        var windowSize = ImGui.GetWindowSize();
        var openPopup = ImGui.ColorButton(label, vectorColor, ImGuiColorEditFlags.NoDragDrop | ImGuiColorEditFlags.AlphaPreview, new Vector2(windowSize.X * 0.65f, 0));

        ImGui.SameLine(0, 4);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, 0);
        ImGui.Text(label);
        ImGui.PopStyleVar();

        if (openPopup)
        {
            ImGui.OpenPopup("Color Picker");
        }

        if (ImGui.BeginPopup("Color Picker"))
        {
            ImGui.ColorPicker4(label, ref vectorColor, ImGuiColorEditFlags.Float);
            value = new Color32(
                (byte)MathF.Round(vectorColor.X * 255),
                (byte)MathF.Round(vectorColor.Y * 255),
                (byte)MathF.Round(vectorColor.Z * 255),
                (byte)MathF.Round(vectorColor.W * 255)
            );

            ImGui.EndPopup();
        }
    }
}

internal unsafe sealed class Vec2Inspector : RangeInspector<Vec2, float>
{
    public Vec2Inspector(FieldInfo field) : base(field)
    {
    }

    protected override RangeAttribute<float> DefaultRange => new(float.MinValue, float.MaxValue, 0.05f);

    protected override void OnSubmitUI(Editor editor, string label, ref Vec2 value)
    {
        ImGui.DragFloat2(label, ref Unsafe.AsRef<Vector2>(Unsafe.AsPointer(ref value)), Range.Speed, Range.Min, Range.Max);
    }
}

internal unsafe sealed class Vec3Inspector : RangeInspector<Vec3, float>
{
    public Vec3Inspector(FieldInfo field) : base(field)
    {
    }

    protected override RangeAttribute<float> DefaultRange => new(float.MinValue, float.MaxValue, 0.05f);


    protected override void OnSubmitUI(Editor editor, string label, ref Vec3 value)
    {
        ImGui.DragFloat3(label, ref Unsafe.AsRef<Vector3>(Unsafe.AsPointer(ref value)), Range.Speed, Range.Min, Range.Max);
    }
}

internal unsafe sealed class Vec4Inspector : RangeInspector<Vec4, float>
{
    public Vec4Inspector(FieldInfo field) : base(field)
    {
    }

    protected override RangeAttribute<float> DefaultRange => new(float.MinValue, float.MaxValue, 0.05f);

    protected override void OnSubmitUI(Editor editor, string label, ref Vec4 value)
    {
        ImGui.DragFloat4(label, ref Unsafe.AsRef<Vector4>(Unsafe.AsPointer(ref value)), Range.Speed, Range.Min, Range.Max);
    }
}

internal unsafe sealed class RectInspector : RangeInspector<Rect, float>
{
    public RectInspector(FieldInfo field) : base(field)
    {
    }

    protected override RangeAttribute<float> DefaultRange => new(float.MinValue, float.MaxValue, 0.05f);

    protected override void OnSubmitUI(Editor editor, string label, ref Rect value)
    {
        ImGui.DragFloat4(label, ref Unsafe.AsRef<Vector4>(Unsafe.AsPointer(ref value)), Range.Speed, Range.Min, Range.Max);
    }
}

internal unsafe sealed class RectIntInspector : RangeInspector<RectInt, int>
{
    public RectIntInspector(FieldInfo field) : base(field)
    {
    }

    protected override RangeAttribute<int> DefaultRange => new(int.MinValue, int.MaxValue, 1);

    protected override void OnSubmitUI(Editor editor, string label, ref RectInt value)
    {
        ImGui.DragInt4(label, ref value.X, Range.Speed, Range.Min, Range.Max);
    }
}