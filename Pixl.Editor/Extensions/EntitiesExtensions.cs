using EntitiesDb;

namespace Pixl.Editor;

public static class EntitiesExtensions
{
    public static Name GetNameOrPrefixed(this EntityDatabase entities, uint entityId, string prefix = "Entity ") => entities.GetNameOrPrefixed(entityId, prefix.AsSpan());
    public static Name GetNameOrPrefixed(this EntityDatabase entities, uint entityId, ReadOnlySpan<char> prefix)
    {
        var name = entities.TryGetComponent<Name>(entityId, out var found);
        if (!found) name = Name.CreateNameWithId(prefix, entityId);
        return name;
    }
}
