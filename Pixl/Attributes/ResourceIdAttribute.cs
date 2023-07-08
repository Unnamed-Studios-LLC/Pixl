namespace Pixl;

[AttributeUsage(AttributeTargets.Field)]
public abstract class ResourceIdAttribute : Attribute
{
    protected ResourceIdAttribute(Type resourceType)
    {
        ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
    }
    public Type ResourceType { get; }
}

public sealed class ResourceIdAttribute<T> : ResourceIdAttribute where T : Resource
{
    public ResourceIdAttribute() : base(typeof(T)) { }
}
