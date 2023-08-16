using EntitiesDb;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pixl;

public abstract class TypeMetaData
{
    private readonly IReadOnlyDictionary<Type, Attribute> _attributeMap;
    private readonly IReadOnlyDictionary<string, FieldMetaData> _fieldMap;

    protected TypeMetaData(string name, bool isComponent, bool isVertex, int size, Attribute[] attributes, FieldMetaData[] fields)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        IsComponent = isComponent;
        IsVertex = isVertex;
        Size = size;
        Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
        Fields = fields ?? throw new ArgumentNullException(nameof(fields));
        _attributeMap = attributes.ToDictionary(x => x.GetType());
        _fieldMap = fields.ToDictionary(x => x.Name);
    }

    public string Name { get; }
    public Attribute[] Attributes { get; }
    public FieldMetaData[] Fields { get; }
    public virtual bool Bufferable => false;

    public bool HasAttribute<T>() => HasAttribute(typeof(T));
    public bool HasAttribute(Type type) => _attributeMap.ContainsKey(type);

    public bool TryGetAttribute<T>(out Attribute? attribute) => TryGetAttribute(typeof(T), out attribute);
    public bool TryGetAttribute(Type type, out Attribute? attribute) => _attributeMap.TryGetValue(type, out attribute);
    public bool TryGetField(string fieldName, out FieldMetaData? field) => _fieldMap.TryGetValue(fieldName, out field);

    internal int Size { get; }
    internal bool IsComponent { get; }
    internal bool IsVertex { get; }

    internal virtual void AddComponent(EntityDatabase entities, uint entityId, object component) { }
    internal abstract object CreateInstance();
    internal virtual object? GetBuffer(EntityDatabase entities, uint entityId) => null;
    internal virtual object? GetComponent(EntityDatabase entities, uint entityId) => null;
    internal virtual IEnumerable<object> GetComponentObjects(EntityDatabase entities, uint entityId) => Enumerable.Empty<object>();
    internal virtual bool HasComponent(EntityDatabase entities, uint entityId) => false;
    internal virtual void SetComponent(EntityDatabase entities, uint entityId, object? value) { }
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
    public UnmanagedTypeMetaData(string name, bool isComponent, bool isVertex, Attribute[] attributes, FieldMetaData[] fields, Func<T> factory) :
        base(name, isComponent, isVertex, attributes, fields, factory) { }

    public override bool Bufferable => EntityDatabase.IsBufferable<T>();

    internal override void AddComponent(EntityDatabase entities, uint entityId, object component)
    {
        if (component is not T typed) return;
        if (Bufferable)
        {
            var buffer = entities.TryGetBuffer<T>(entityId, out var found);
            if (found)
            {
                buffer.Add(typed);
            }
            else
            {
                Span<T> span = stackalloc T[1];
                span[0] = typed;
                entities.AddBuffer<T>(entityId, span);
            }
        }
        else
        {
            entities.AddComponent(entityId, typed);
        }
    }

    internal override bool HasComponent(EntityDatabase entities, uint entityId) => entities.HasComponent<T>(entityId);

    internal override object? GetBuffer(EntityDatabase entities, uint entityId)
    {
        var buffer = entities.GetBuffer<T>(entityId);
        return buffer;
    }

    internal override object? GetComponent(EntityDatabase entities, uint entityId)
    {
        ref var component = ref entities.GetComponent<T>(entityId);
        return component;
    }

    internal override IEnumerable<object> GetComponentObjects(EntityDatabase entities, uint entityId)
    {
        if (Bufferable)
        {
            var buffer = entities.GetBuffer<T>(entityId);
            for (int i = 0; i < buffer.Length; i++)
            {
                yield return buffer[i];
            }
        }
        else
        {
            var component = entities.GetComponent<T>(entityId);
            yield return component;
        }
    }

    internal override void SetComponent(EntityDatabase entities, uint entityId, object? value)
    {
        if (Bufferable) throw new Exception("Cannot SetComponent for bufferable component");
        if (value is not T typed) throw new ArgumentException("Invalid argument type", nameof(value));
        ref var component = ref entities.GetComponent<T>(entityId);
        component = typed;
    }
}