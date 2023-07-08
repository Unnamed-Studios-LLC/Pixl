using EntitiesDb;
using Veldrid;

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

    public EntityDatabase Entities { get; } = new();

    internal Game Game { get; }

    public void AddDefaultSystems()
    {
        AddSystem<CanvasSystem>();
        AddSystem<NameSystem>();
        AddSystem<CameraSystem>();
        AddSystem<TransformSystem>();
    }

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

    internal void FixedUpdate()
    {
        // fixed update
        _systems.FixedUpdate();
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
