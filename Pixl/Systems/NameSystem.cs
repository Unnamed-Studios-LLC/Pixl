using EntitiesDb;
using System.Collections.Concurrent;

namespace Pixl;

public sealed class NameSystem : ComponentSystem
{
    private readonly ConcurrentDictionary<uint, string> _names = new();

    public string? GetName(uint entityId)
    {
        if (!Scene.Entities.EntityExists(entityId) ||
            !Scene.Entities.HasComponent<Editable>(entityId)) return null;
        string? name;
        while (!_names.TryGetValue(entityId, out name))
        {
            name = $"Entity {entityId}";
            if (_names.TryAdd(entityId, name)) break;
        }
        return name;
    }

    public override void OnRegisterEvents()
    {
        RegisterEvent<Editable>(Event.OnRemove, OnRemove);
    }

    public void SetName(uint entityId, string name)
    {
        if (!Scene.Entities.EntityExists(entityId)) return;
        _names[entityId] = name;
    }

    private void OnRemove(uint entityId, ref Editable editable)
    {
        _names.TryRemove(entityId, out _);
    }
}
