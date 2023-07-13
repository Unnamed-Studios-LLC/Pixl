using EntitiesDb;
using System.Collections.Concurrent;

namespace Pixl;

public sealed class NameSystem : ComponentSystem
{
    internal const string TempPrefix = "Entity ";

    private readonly ConcurrentDictionary<uint, string> _names = new();

    public string? GetName(uint entityId)
    {
        if (!Scene.Entities.EntityExists(entityId) ||
            !Scene.Entities.HasComponent<Named>(entityId)) return null;
        if (!_names.TryGetValue(entityId, out var name)) return null;
        return name;
    }

    public override void OnRegisterEvents()
    {
        RegisterEvent<Named>(Event.OnRemove, OnRemove);
    }

    public void SetName(uint entityId, string name)
    {
        if (!Scene.Entities.EntityExists(entityId)) return;
        _names[entityId] = name;
    }

    private void OnRemove(uint entityId, ref Named editable)
    {
        _names.TryRemove(entityId, out _);
    }
}
