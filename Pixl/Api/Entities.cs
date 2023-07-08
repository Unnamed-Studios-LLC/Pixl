using EntitiesDb;
using System.Runtime.CompilerServices;

namespace Pixl;
/*
public sealed partial class Entities : IDisposable
{
    private readonly EntityDatabase _entities = new();

    public ref T AddComponent<T>(uint entityId, T data = default) where T : unmanaged
    {
        return ref _entities.AddComponent(entityId, data);
    }

    public void ApplyLayout(uint entityId, EntityLayout layout)
    {
        _entities.ApplyLayout(entityId, layout.Layout);
    }

    public uint CloneEntity(uint entityId) => _entities.CloneEntity(entityId);

    public uint CreateEntity() => _entities.CreateEntity(0);
    public uint CreateEntity(uint entityId) => _entities.CreateEntity(entityId);

    public uint CreateEntity(EntityLayout layout) => _entities.CreateEntity(0, layout.Layout);
    public uint CreateEntity(uint entityId, EntityLayout layout) => _entities.CreateEntity(entityId, layout.Layout);

    public void DestroyAllEntities() => _entities.DestroyAllEntities();

    public bool DestroyEntity(uint entityId) => _entities.DestroyEntity(entityId);

    public void DisableEntity(uint entityId) => _entities.DisableEntity(entityId);

    public void Dispose() => _entities.Dispose();

    public void EnableEntity(uint entityId) => _entities.EnableEntity(entityId);

    public bool EntityExists(uint entityId) => _entities.EntityExists(entityId);

    public ref T GetComponent<T>(uint entityId) where T : unmanaged => ref _entities.GetComponent<T>(entityId);

    public Entity GetEntity(uint entityId) => _entities.GetEntity(entityId);

    public bool HasComponent<T>(uint entityId) where T : unmanaged => _entities.HasComponent<T>(entityId);

    public void RemoveAllEventHandlers() => _entities.RemoveAllEventHandlers();

    public unsafe bool RemoveComponent<T>(uint entityId) where T : unmanaged => _entities.RemoveComponent<T>(entityId);

    public ref T TryGetComponent<T>(uint entityId, out bool found) where T : unmanaged
    {
        if (!_entities.HasComponent<T>(entityId))
        {
            found = false;
            return ref Unsafe.NullRef<T>();
        }

        found = true;
        return ref GetComponent<T>(entityId);
    }
}
*/
