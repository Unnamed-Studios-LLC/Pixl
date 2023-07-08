using System.Collections.Concurrent;

namespace Pixl;

internal sealed class Resources
{
    private readonly ConcurrentDictionary<long, Resource> _resources = new();

    public Resources(Graphics graphics, Files files)
    {
        Graphics = graphics;
        Default = new DefaultResources(files);
        Default.Add(this);
    }

    internal Graphics Graphics { get; }
    internal DefaultResources Default { get; }

    public IEnumerable<Resource> All => _resources.Values;

    public void Add(Resource resource)
    {
        GenerateId(resource);
        AddCore(resource);
    }

    public void Add(Resource resource, long id, bool requireId)
    {
        if (!_resources.TryAdd(id, resource))
        {
            if (requireId) throw new Exception($"Unable to add resource, given id is taken");
            GenerateId(resource);
        }
        else resource.Id = id;
        AddCore(resource);
    }

    public Resource Get(long id) => _resources[id];

    public bool Remove(Resource resource) => Remove(resource.Id);
    public bool Remove(long id)
    {
        if (!_resources.TryRemove(id, out var resource)) return false;
        try
        {
            resource.OnRemove();
        }
        finally
        {
            resource.Id = 0;
            resource.Resources = null;
        }
        return true;
    }

    public bool TryGet(long id, out Resource? resource) => _resources.TryGetValue(id, out resource);

    private void GenerateId(Resource resource)
    {
        long id;
        do
        {
            id = Random.Shared.NextInt64(100_000_000, long.MaxValue);
        }
        while (id == 0 || !_resources.TryAdd(id, resource));
        resource.Id = id;
    }

    private void AddCore(Resource resource)
    {
        resource.Resources = this;

        try
        {
            resource.OnAdd();
        }
        catch
        {
            _resources.TryRemove(resource.Id, out _);
            resource.Id = 0;
            resource.Resources = null;
            throw;
        }
    }
}
