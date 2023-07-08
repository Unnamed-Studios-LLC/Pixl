namespace Pixl;

public struct EntityId
{
    public static readonly EntityId Zero = new(0);

    public uint Raw;

    public EntityId(uint raw)
    {
        Raw = raw;
    }

    public static implicit operator EntityId(uint raw) => new(raw);
}
