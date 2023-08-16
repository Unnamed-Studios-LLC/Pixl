using EntitiesDb;

namespace Pixl.Editor;

internal abstract class ComponentBufferInspector : ListInspector
{
    protected ComponentBufferInspector(ObjectInspector elementInspector) : base(elementInspector)
    {
    }
}

internal sealed class ComponentBufferInspector<T> : ComponentBufferInspector where T : unmanaged
{
    public ComponentBufferInspector() : base(new DefaultObjectInspector(typeof(T)))
    {
    }

    protected override object AddElement(object @object, int index)
    {
        var buffer = (ComponentBuffer<T>)@object;
        buffer.Add(new T());
        return buffer;
    }

    protected override object GetElement(object @object, int index)
    {
        var buffer = (ComponentBuffer<T>)@object;
        return buffer[index];
    }

    protected override int GetLength(object @object)
    {
        var buffer = (ComponentBuffer<T>)@object;
        return buffer.Length;
    }

    protected override object RemoveElement(object @object, int index)
    {
        var buffer = (ComponentBuffer<T>)@object;
        buffer.RemoveAtSwapBack(index);
        return buffer[index];
    }

    protected override object SetElement(object @object, int index, object value)
    {
        var buffer = (ComponentBuffer<T>)@object;
        buffer[index] = (T)value;
        return buffer;
    }
}
