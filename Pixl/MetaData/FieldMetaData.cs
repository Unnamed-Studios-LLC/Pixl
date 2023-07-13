namespace Pixl;

public delegate void FieldSetter<TInstance, TField>(ref TInstance instance, in TField field);

public abstract class FieldMetaData
{
    protected FieldMetaData(string name, Type type, Attribute[] attributes)
    {
        Name = name;
        Type = type;
        Attributes = attributes;
    }

    public string Name { get; }
    public Type Type { get; }
    public Attribute[] Attributes { get; }

    public abstract bool TryGetValue(object instance, out object? value);
    public abstract bool TrySetValue(ref object instance, object? value);
}

public sealed class FieldMetaData<TInstance, TField> : FieldMetaData
{
    private readonly Func<TInstance, TField>? _getter;
    private readonly FieldSetter<TInstance, TField>? _setter;

    public FieldMetaData(string name, Attribute[] attributes, Func<TInstance, TField>? getter, FieldSetter<TInstance, TField>? setter) : base(name, typeof(TField), attributes)
    {
        _getter = getter;
        _setter = setter;
    }

    public override bool TryGetValue(object instance, out object? value)
    {
        if (_getter == null ||
            instance is not TInstance typedInstance)
        {
            value = null;
            return false;
        }

        value = _getter(typedInstance);
        return true;
    }

    public override bool TrySetValue(ref object instance, object? value)
    {
        if (_setter == null ||
            instance is not TInstance typedInstance ||
            value is not TField typedValue)
        {
            return false;
        }
        
        _setter(ref typedInstance, in typedValue);
        instance = typedInstance!;
        return true;
    }
}