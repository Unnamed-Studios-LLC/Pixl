namespace Pixl;

[Serializable]
public struct EntityHeader
{
    public uint Id;
    public string? Name;

    public EntityHeader(uint id, string? name)
    {
        Id = id;
        Name = name;
    }
}
