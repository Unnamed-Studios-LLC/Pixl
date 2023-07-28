namespace Pixl;

[Serializable]
[Inline]
public struct Entity
{
    public static readonly Entity Zero = new(0);

    public uint Id;

    public Entity(uint id)
    {
        Id = id;
    }

    public static bool operator ==(in Entity lh, in Entity rh) => lh.Id == rh.Id;
    public static bool operator !=(in Entity lh, in Entity rh) => lh.Id != rh.Id;

    public static implicit operator Entity(uint id) => new(id);

    public override bool Equals(object? obj)
    {
        if (obj is Entity other) return this == other;
        return false;
    }

    public override int GetHashCode() => (int)Id;
}
