using System.Reflection;

namespace Pixl.Editor;

internal unsafe abstract class ObjectInspector
{
    public abstract object? SubmitUI(Editor editor, string label, object @object);
}

internal unsafe abstract class ObjectInspector<T> : ObjectInspector
{
    public override unsafe object? SubmitUI(Editor editor, string label, object @object)
    {
        if (@object is not T tValue) return @object;
        OnSubmitUI(editor, label, ref tValue);
        return tValue;
    }

    protected abstract void OnSubmitUI(Editor editor, string label, ref T value);
}

internal unsafe abstract class RangeInspector<T, TRange> : ObjectInspector
{
    public RangeInspector(FieldInfo field)
    {
        Range = field.GetCustomAttribute<RangeAttribute<TRange>>() is RangeAttribute<TRange> range ?
            range : DefaultRange;
    }

    protected RangeAttribute<TRange> Range { get; }
    protected abstract RangeAttribute<TRange> DefaultRange { get; }

    public override unsafe object? SubmitUI(Editor editor, string label, object @object)
    {
        if (@object is not T tValue) return @object;
        OnSubmitUI(editor, label, ref tValue);
        return tValue;
    }

    protected abstract void OnSubmitUI(Editor editor, string label, ref T value);
}