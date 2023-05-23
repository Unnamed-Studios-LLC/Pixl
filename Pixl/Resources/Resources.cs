namespace Pixl;

internal sealed class Resources
{
    private readonly Dictionary<uint, Resource> _resources = new();
    private uint _nextResourceId = 1;

    public IEnumerable<Resource> All => _resources.Values;

    public void Add(Resource resource)
    {
        // TODO thread check
        var id = GenerateId();
        _resources.Add(id, resource);
        resource.Id = id;
        resource.Resources = this;
        ImplUtils.CallUserMethod(resource.OnAdd);
    }

    public Resource Get(uint id) => _resources[id];

    public bool Remove(Resource resource) => Remove(resource.Id);
    public bool Remove(uint id)
    {
        // TODO thread check
        if (!_resources.Remove(id, out var resource)) return false;
        ImplUtils.CallUserMethod(resource.OnRemove);
        resource.Id = 0;
        resource.Resources = null;
        return true;
    }

    public bool TryGet(uint id, out Resource? resource) => _resources.TryGetValue(id, out resource);

    private uint GenerateId()
    {
        uint id;
        do
        {
            id = _nextResourceId++;
        }
        while (id == 0 || _resources.ContainsKey(id));
        return id;
    }
}
