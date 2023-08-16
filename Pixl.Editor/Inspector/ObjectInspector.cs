using EntitiesDb;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pixl.Editor;

internal unsafe abstract class ObjectInspector
{
    private readonly static Type s_genericComponentBufferType = typeof(ComponentBufferInspector<>);

    public static ObjectInspector Create(Type type)
    {
        ObjectInspector? inspector = null;
        if (MetaData.TryGet(type, out var metaData) &&
            metaData != null &&
            metaData.IsComponent &&
            metaData.Bufferable)
        {
            var inspectorType = s_genericComponentBufferType.MakeGenericType(type);
            inspector = (ObjectInspector?)Activator.CreateInstance(inspectorType);
            return inspector ?? throw new Exception($"Unable to create inspector for type: {type}");
        }

        inspector ??= ValueInspectors.GetInspector(type, type.GetCustomAttributes());
        return inspector ?? new DefaultObjectInspector(type);
    }

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
    public RangeInspector(IEnumerable<Attribute> attributes)
    {
        Range = attributes.OfType<RangeAttribute<TRange>>().FirstOrDefault() ?? DefaultRange;
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