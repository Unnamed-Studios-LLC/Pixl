using EntitiesDb;
using Veldrid;

namespace Pixl;

public sealed class Scene
{
    private readonly SystemList _systems = new();
    private readonly VertexRenderer _renderer;

    internal Scene(Resources resources, DefaultResources defaultResources)
    {
        _renderer = new VertexRenderer(resources, defaultResources, ushort.MaxValue, 2_048_000);
    }

    public EntityDatabase Entities { get; } = new();

    public void AddSystem(ComponentSystem system)
    {
        if (system.HasScene) throw new Exception("ComponentSystem already added to a Scene");
        _systems.Add(system);
        system.SetScene(this);
        system.Registering = true;
        system.Removing = false;
        ImplUtils.CallUserMethod(system.OnRegisterEvents);
        system.Registering = false;
        ImplUtils.CallUserMethod(system.OnAdd);
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
        _systems.Remove(system);
        ImplUtils.CallUserMethod(system.OnRemove);
        system.Registering = true;
        system.Removing = true;
        ImplUtils.CallUserMethod(system.OnRegisterEvents);
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
        commands.Begin();
        commands.SetFramebuffer(frameBuffer);
        commands.ClearColorTarget(0, RgbaFloat.Black);
        commands.End();
        graphics.Submit(commands);

        _renderer.Begin(graphics, commands, frameBuffer);
        _systems.Render(_renderer);
        _renderer.End();
    }

    internal void Start(Game game)
    {
        game.Resources.Add(_renderer);
    }

    internal void Update()
    {
        // update
        _systems.Update();

        // late update
        _systems.LateUpdate();
    }
}
