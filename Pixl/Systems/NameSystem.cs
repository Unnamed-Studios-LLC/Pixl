using EntitiesDb;
using System.Collections.Concurrent;

namespace Pixl;

public sealed class NameSystem : ComponentSystem
{
    private readonly ConcurrentDictionary<uint, string> _names = new();

    public string? GetName(uint entityId)
    {
        if (!Scene.Entities.EntityExists(entityId)) return null;
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
        RegisterEvent<Transform>(Event.OnRemove, OnRemove);
        RegisterEvent<CanvasTransform>(Event.OnRemove, OnRemove);
    }

    public void SetName(uint entityId, string name)
    {
        if (!Scene.Entities.EntityExists(entityId)) return;
        _names[entityId] = name;
    }

    private void OnRemove(uint entityId, ref Transform transform)
    {
        _names.TryRemove(entityId, out _);
    }

    private void OnRemove(uint entityId, ref CanvasTransform transform)
    {
        _names.TryRemove(entityId, out _);
    }
}
