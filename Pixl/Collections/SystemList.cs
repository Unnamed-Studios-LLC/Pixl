﻿namespace Pixl;

internal sealed class SystemList
{
    private static readonly Type s_baseType = typeof(ComponentSystem);

    private readonly List<ComponentSystem> _systems = new();
    private readonly List<ComponentSystem> _onFixedUpdate = new();
    private readonly List<ComponentSystem> _onLateUpdate = new();
    private readonly List<ComponentSystem> _onRender = new();
    private readonly List<ComponentSystem> _onUpdate = new();

    public void Add(ComponentSystem system)
    {
        _systems.Add(system);
        _systems.Sort();

        var type = system.GetType();
        if (type.DoesOverride(s_baseType, nameof(ComponentSystem.OnFixedUpdate)))
        {
            _onFixedUpdate.Add(system);
            _onFixedUpdate.Sort();
        }

        if (type.DoesOverride(s_baseType, nameof(ComponentSystem.OnLateUpdate)))
        {
            _onLateUpdate.Add(system);
            _onLateUpdate.Sort();
        }

        if (type.DoesOverride(s_baseType, nameof(ComponentSystem.OnRender), typeof(VertexRenderer)))
        {
            _onRender.Add(system);
            _onRender.Sort();
        }

        if (type.DoesOverride(s_baseType, nameof(ComponentSystem.OnUpdate)))
        {
            _onUpdate.Add(system);
            _onUpdate.Sort();
        }
    }

    public T? GetFirst<T>() where T : class
    {
        return _systems.FirstOrDefault(x => x is T) as T;
    }

    public IEnumerable<ComponentSystem> GetAll() => _systems;
    public IEnumerable<T> GetAll<T>() where T : class
    {
        return _systems.Where(x => x is T).Select(x => (x as T)!);
    }

    public void FixedUpdate()
    {
        foreach (var system in _onFixedUpdate)
        {
            try
            {
                system.OnFixedUpdate();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    public void LateUpdate()
    {
        foreach (var system in _onLateUpdate)
        {
            try
            {
                system.OnLateUpdate();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    public void Remove(ComponentSystem system)
    {
        // TODO ensure main thread
        _systems.Remove(system);
        _systems.Sort();

        var type = system.GetType();
        if (type.DoesOverride(s_baseType, nameof(ComponentSystem.OnFixedUpdate)))
        {
            _onFixedUpdate.Remove(system);
            _onFixedUpdate.Sort();
        }

        if (type.DoesOverride(s_baseType, nameof(ComponentSystem.OnLateUpdate)))
        {
            _onLateUpdate.Remove(system);
            _onLateUpdate.Sort();
        }

        if (type.DoesOverride(s_baseType, nameof(ComponentSystem.OnRender)))
        {
            _onRender.Remove(system);
            _onRender.Sort();
        }

        if (type.DoesOverride(s_baseType, nameof(ComponentSystem.OnUpdate)))
        {
            _onUpdate.Remove(system);
            _onUpdate.Sort();
        }
    }

    public void Render(VertexRenderer renderer)
    {
        foreach (var system in _onRender)
        {
            try
            {
                system.OnRender(renderer);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    public void Update()
    {
        foreach (var system in _onUpdate)
        {
            try
            {
                system.OnUpdate();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }
}
