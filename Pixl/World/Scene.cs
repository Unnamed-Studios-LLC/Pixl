﻿using EntitiesDb;
using Veldrid;

namespace Pixl
{
    public class Scene
    {
        private readonly SystemList _systems = new();
        private readonly VertexRenderer<Vertex> _renderer;

        public Scene()
        {
            _renderer = new VertexRenderer<Vertex>(30_000);
        }

        public EntityDatabase Entities { get; } = new();

        public void AddSystem(ComponentSystem system)
        {
            if (system.HasScene) throw new Exception("ComponentSystem already added to a Scene");
            _systems.Add(system);
            system.SetScene(this);
            system.Registering = true;
            system.Removing = false;
            InternalUtils.CallUserMethod(system.OnRegisterEvents);
            system.Registering = false;
            InternalUtils.CallUserMethod(system.OnAdd);
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
            InternalUtils.CallUserMethod(system.OnRemove);
            system.Registering = true;
            system.Removing = true;
            InternalUtils.CallUserMethod(system.OnRegisterEvents);
            system.Registering = false;
            system.SetScene(null);
            return true;
        }

        internal void FixedUpdate()
        {
            // fixed update
            _systems.FixedUpdate();
        }

        internal void Render(CommandList commands, Framebuffer frameBuffer)
        {
            commands.Begin();
            commands.SetFramebuffer(frameBuffer);
            commands.ClearColorTarget(0, RgbaFloat.Black);
            commands.End();
            Game.Current.Graphics.Submit(commands);

            _renderer.Begin(commands, frameBuffer);
            _systems.Render(_renderer);
            _renderer.EndBatch();
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
}
