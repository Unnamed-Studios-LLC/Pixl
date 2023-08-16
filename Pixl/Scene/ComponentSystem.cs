using EntitiesDb;

namespace Pixl
{
    public abstract class ComponentSystem : IComparable<ComponentSystem>
    {
        private Scene? _scene;

        public EntityDatabase Entities => Scene.Entities;
        /// <summary>
        /// If this <see cref="ComponentSystem"/> should be removed when a new scene is loaded
        /// </summary>
        public bool Global { get; set; } = false;
        /// <summary>
        /// The execution order of this <see cref="ComponentSystem"/>
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// The <see cref="Scene"/> this <see cref="ComponentSystem"/> is added to
        /// </summary>
        public Scene Scene => _scene ?? throw new Exception($"This {nameof(ComponentSystem)} has not been added to a {nameof(Scene)}");

        internal bool HasScene => _scene != null;
        internal bool Registering { get; set; } = false;
        internal bool Removing { get; set; } = false;

        public int CompareTo(ComponentSystem? other)
        {
            if (other == null) return -1;
            return Order - other.Order;
        }

        /// <summary>
        /// Executed only once when added to the <see cref="Scene"/>
        /// </summary>
        public virtual void OnAdd() { }
        /// <summary>
        /// Executed only once when added to the <see cref="Scene"/>
        /// </summary>
        public virtual void OnFixedUpdate() { }
        public virtual void OnLateUpdate() { }
        public virtual void OnRemove() { }
        public virtual void OnRegisterEvents() { }
        public virtual void OnUpdate() { }

        internal virtual void OnRender(VertexRenderer renderer) { }

        internal void SetScene(Scene? scene)
        {
            _scene = scene;
        }

        /// <summary>
        /// Registers a component event callback
        /// </summary>
        /// <typeparam name="T">The component type</typeparam>
        /// <param name="event">The type of event to register</param>
        /// <param name="componentHandler">Component handler method called on event</param>
        protected void RegisterEvent<T>(Event @event, ComponentHandler<T> componentHandler) where T : unmanaged
        {
            if (!Registering) throw new Exception($"{nameof(RegisterEvent)} may only be called from within {nameof(ComponentSystem)}.{nameof(OnRegisterEvents)}");
            if (!Removing) Scene?.Entities.AddEvent(@event, componentHandler);
            else Scene?.Entities.AddEvent(@event, componentHandler);
        }

        /// <summary>
        /// Registers an entity event callback
        /// </summary>
        /// <param name="event">The type of event to register</param>
        /// <param name="componentHandler">Component handler method called on event</param>
        protected void RegisterEvent(Event @event, EntityHandler entityHandler)
        {
            if (!Registering) throw new Exception($"{nameof(RegisterEvent)} may only be called from within {nameof(ComponentSystem)}.{nameof(OnRegisterEvents)}");
            if (!Removing) Scene?.Entities.AddEvent(@event, entityHandler);
            else Scene?.Entities.AddEvent(@event, entityHandler);
        }
    }
}
