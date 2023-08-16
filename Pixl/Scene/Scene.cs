using EntitiesDb;
using Veldrid;
using YamlDotNet.RepresentationModel;

namespace Pixl;

public sealed class Scene
{
    private readonly SystemList _systems = new();
    private readonly VertexRenderer _renderer;

    internal Scene(Game game)
    {
        Game = game;
        _renderer = new VertexRenderer(game.Resources, ushort.MaxValue, 2_048_000);
    }

    public EntityDatabase Entities { get; private set; } = new();

    internal Game Game { get; }
    internal Dictionary<uint, List<YamlDocument>> FailedComponents { get; } = new();
    internal List<YamlDocument> FailedSystems { get; } = new();

    public void AddSystem(ComponentSystem system)
    {
        if (system.HasScene) throw new Exception("ComponentSystem already added to a Scene");
        Game.RequireMainThread();

        _systems.Add(system);
        system.SetScene(this);
        system.Removing = false;
        system.Registering = true;
        try
        {
            system.OnRegisterEvents();
        }
        catch (Exception e)
        {
            Game.Logger.Log(e);
        }
        system.Registering = false;

        try
        {
            system.OnAdd();
        }
        catch (Exception e)
        {
            Game.Logger.Log(e);
        }
    }
    public ComponentSystem AddSystem<T>() where T : ComponentSystem, new()
    {
        var system = new T();
        AddSystem(system);
        return system;
    }

    public T? GetSystem<T>() where T : class => _systems.GetFirst<T>();
    public IEnumerable<ComponentSystem> GetSystems() => _systems.GetAll();
    public IEnumerable<T> GetSystems<T>() where T : class => _systems.GetAll<T>();

    public bool RemoveSystem(ComponentSystem system)
    {
        if (system.Scene != this) return false;
        Game.RequireMainThread();

        _systems.Remove(system);
        try
        {
            system.OnAdd();
        }
        catch (Exception e)
        {
            Game.Logger.Log(e);
        }

        system.Removing = true;
        system.Registering = true;
        try
        {
            system.OnRegisterEvents();
        }
        catch (Exception e)
        {
            Game.Logger.Log(e);
        }
        system.Registering = false;
        system.SetScene(null);
        return true;
    }

    internal void Clear()
    {
        _systems.Clear();
        Entities.Clear();
        FailedComponents.Clear();
        FailedSystems.Clear();
    }

    internal void FixedUpdate()
    {
        // fixed update
        _systems.FixedUpdate();
    }

    internal void GetDocuments(List<YamlDocument> documentList)
    {
        static YamlDocument? getDocument(object value, string tag)
        {
            var type = value.GetType();
            if (!MetaData.TryGet(type, out var metaData) ||
                metaData == null) return null;

            var document = Serializer.GetDocument(metaData.Name, value, tag);
            if (document == null) return null;
            return document;
        }

        foreach (var system in _systems.GetAll())
        {
            var document = getDocument(system, "!system");
            if (document == null) continue;
            documentList.Add(document);
        }

        documentList.AddRange(FailedSystems);

        Entities.IncludeDisabled().ForEach((uint entityId) =>
        {
            var header = getDocument(new Entity(entityId), "!entity");
            if (header == null) return;
            documentList.Add(header);

            foreach (var type in Entities.GetComponentTypes(entityId))
            {
                var metaData = MetaData.Get(type);
                foreach (var component in metaData.GetComponentObjects(Entities, entityId))
                {
                    var document = getDocument(component, "!component");
                    if (document == null) return;
                    documentList.Add(document);
                }
            }

            if (FailedComponents.TryGetValue(entityId, out var failedList))
            {
                documentList.AddRange(failedList);
            }
        });
    }

    internal void LoadDocuments(IEnumerable<YamlDocument> documents)
    {
        Entity currentEntity = default;
        foreach (var document in documents)
        {
            var parsed = Serializer.GetObject(document, out var hint);
            if (parsed == null)
            {
                // TODO failure to parse
                switch (hint)
                {
                    case "!system":
                        FailedSystems.Add(document);
                        break;
                    case "!entity":

                        break;
                    case "!component":
                        if (!FailedComponents.TryGetValue(currentEntity.Id, out var failedComponents))
                        {
                            failedComponents = new();
                            FailedComponents.Add(currentEntity.Id, failedComponents);
                        }
                        failedComponents.Add(document);
                        break;
                }
                continue;
            }

            if (parsed is ComponentSystem system)
            {
                AddSystem(system);
            }
            else if (parsed is Entity entityHeader)
            {
                currentEntity = entityHeader;
                Entities.CreateEntity(entityHeader.Id);
            }
            else if (parsed is IComponent component &&
                MetaData.TryGet(parsed.GetType(), out var metaData) &&
                metaData != null &&
                currentEntity.Id != 0)
            {
                metaData.AddComponent(Entities, currentEntity.Id, component);
            }
        }
    }

    internal void Render(Graphics graphics, CommandList commands, Framebuffer frameBuffer)
    {
        _renderer.Begin(graphics, commands, frameBuffer);
        _systems.Render(_renderer);
        _renderer.End();
    }

    internal void Start()
    {
        Game.Resources.Add(_renderer);
    }

    internal void Stop()
    {
        Clear();
        Game.Resources.Remove(_renderer);
    }

    internal void Update()
    {
        // update
        _systems.Update();

        // late update
        _systems.LateUpdate();
    }
}
