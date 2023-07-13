using EntitiesDb;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pixl;

public abstract class TypeMetaData
{
    private readonly IReadOnlyDictionary<string, FieldMetaData> _fieldMap;

    protected TypeMetaData(string name, bool isComponent, bool isVertex, int size, Attribute[] attributes, FieldMetaData[] fields)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        IsComponent = isComponent;
        IsVertex = isVertex;
        Size = size;
        Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
        Fields = fields ?? throw new ArgumentNullException(nameof(fields));
        _fieldMap = fields.ToDictionary(x => x.Name);
    }

    public string Name { get; }
    public Attribute[] Attributes { get; }
    public FieldMetaData[] Fields { get; }

    public bool TryGetField(string fieldName, out FieldMetaData? field) => _fieldMap.TryGetValue(fieldName, out field);

    internal int Size { get; }
    internal bool IsComponent { get; }
    internal bool IsVertex { get; }

    internal virtual void AddComponent(EntityDatabase entities, uint entityId, object component) { }
    internal virtual bool HasComponent(EntityDatabase entities, uint entityId) => false;
    internal abstract object CreateInstance();
}

public class TypeMetaData<T> : TypeMetaData
{
    private readonly Func<T> _factory;

    public TypeMetaData(string name, bool isComponent, bool isVertex, Attribute[] attributes, FieldMetaData[] fields, Func<T> factory) :
        base(name, isComponent, isVertex, RuntimeHelpers.IsReferenceOrContainsReferences<T>() ? -1 : Marshal.SizeOf<T>(), attributes, fields)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    internal override object CreateInstance() => _factory.Invoke()!;
}

public sealed class UnmanagedTypeMetaData<T> : TypeMetaData<T> where T : unmanaged
{
    public UnmanagedTypeMetaData(string name, bool isComponent, bool isVertex, Attribute[] attributes, FieldMetaData[] fields, Func<T> factory) : base(name, isComponent, isVertex, attributes, fields, factory) { }

    internal override void AddComponent(EntityDatabase entities, uint entityId, object component)
    {
        if (component is not T typed) return;
        entities.AddComponent(entityId, typed);
    }

    internal override bool HasComponent(EntityDatabase entities, uint entityId) => entities.HasComponent<T>(entityId);
}