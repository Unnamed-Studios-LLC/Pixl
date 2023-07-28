namespace Pixl;

public struct ResourceView
{
    public uint Id;

    public ResourceView(uint id)
    {
        Id = id;
    }

    public Resource? Get()
    {
        return null;
    }
    public TResource? Get<TResource>() where TResource : Resource
    {
        throw new NotImplementedException();
    }

    public static bool operator ==(in ResourceView lh, in ResourceView rh) => lh.Id == rh.Id;
    public static bool operator !=(in ResourceView lh, in ResourceView rh) => lh.Id != rh.Id;

    public static implicit operator ResourceView(uint id) => new(id);

    public override bool Equals(object? obj)
    {
        if (obj is ResourceView other) return this == other;
        return false;
    }

    public override int GetHashCode() => (int)Id;
}
