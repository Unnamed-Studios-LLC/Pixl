namespace Pixl;

[AttributeUsage(AttributeTargets.Field)]
public abstract class ResourceTypeAttribute : Attribute
{
    protected ResourceTypeAttribute(Type resourceType)
    {
        ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
    }
    public Type ResourceType { get; }
}

public sealed class ResourceTypeAttribute<T> : ResourceTypeAttribute where T : Resource
{
    public ResourceTypeAttribute() : base(typeof(T)) { }
}
