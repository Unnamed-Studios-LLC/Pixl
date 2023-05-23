using EntitiesDb;
using Pixl.Demo.Components;

namespace Pixl.Demo.Systems
{
    internal sealed class VelocitySystem : ComponentSystem
    {
        public uint CameraEntityId;
        private readonly EntityLayout _cameraLayout;
        private readonly EntityLayout _canvasLayout;
        private readonly EntityLayout _entityLayout;
        private readonly EntityLayout _uiLayout;
        private readonly List<uint> _entityIds = new();
        private readonly Texture2d _charactersTexture;

        private int _fps;

        public VelocitySystem()
        {
            _cameraLayout = EntityLayoutBuilder.Create()
                .Add<Transform>()
                .Add<Camera>()
                .Build();

            _canvasLayout = EntityLayoutBuilder.Create()
                .Add<Canvas>()
                .Build();

            _entityLayout = EntityLayoutBuilder.Create()
                .Add<Transform>()
                .Add<Sprite>()
                .Add<Velocity>()
                .Build();

            _uiLayout = EntityLayoutBuilder.Create()
                .Add<CanvasTransform>()
                .Add<Sprite>()
                .Build();

            var fileStream = File.OpenRead("Assets/characters.png");
            _charactersTexture = Texture2d.CreateFromFile(fileStream, SampleMode.Point, ColorFormat.Rgba32);
        }

        public float SpeedScalar = 1;

        public void CreateEntities(int count)
        {
            var rnd = new Random();
            for (int i = 0; i < count; i++)
            {
                var heading = (float)rnd.NextDouble() * MathF.PI * 2;
                var speed = (float)rnd.NextDouble() * 5;
                var vector = new Vec2(MathF.Sin(heading), MathF.Cos(heading)) * speed;
                var position = new Vec2(-100 + (float)rnd.NextDouble() * 200, -100 + (float)rnd.NextDouble() * 200);
                Vec2 scale = (0.5f + (float)rnd.NextDouble()) * 0.05f;
                var color = new Color32((byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256), 255);

                _entityLayout.Set(new Transform(position, Vec3.Zero, scale));
                _entityLayout.Set(new Sprite(_charactersTexture.Id, new RectInt(0, 0, 8, 8), color));
                _entityLayout.Set(new Velocity(vector));
                var entityId = Scene.Entities.CreateEntity(_entityLayout);
                _entityIds.Add(entityId);
            }
            UpdateTitle();
        }

        public void RemoveEntities(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (_entityIds.Count == 0) break;
                var id = _entityIds[^1];
                Scene.Entities.DestroyEntity(id);
                _entityIds.RemoveAt(_entityIds.Count - 1);
            }
            UpdateTitle();
        }

        public override void OnAdd()
        {
            // create camera
            _cameraLayout.Set(new Transform(new Vec3(0, 0, 10), new Vec3(0, 0, 0), Vec3.One));
            _cameraLayout.Set(new Camera());
            CameraEntityId = Scene.Entities.CreateEntity(_cameraLayout);

            // create canvas
            _canvasLayout.Set(new Canvas(new Vec2(1, 1)));
            Scene.Entities.CreateEntity(_canvasLayout);

            // create ui
            _uiLayout.Set(new CanvasTransform(new Vec2(10, 10), Vec3.Zero, Vec2.One, new Vec2(100, 100), Vec2.Zero, Vec2.Zero, 0));
            _uiLayout.Set(new Sprite(_charactersTexture.Id, new RectInt(0, 0, 32, 32), Color32.White));
            Scene.Entities.CreateEntity(_uiLayout);

            _uiLayout.Set(new CanvasTransform(new Vec2(120, 10), Vec3.Zero, Vec2.One, new Vec2(20, 20), Vec2.Zero, Vec2.Zero, 0));
            _uiLayout.Set(new Sprite(0, new RectInt(0, 0, 32, 32), Color32.White));
            Scene.Entities.CreateEntity(_uiLayout);

            _uiLayout.Set(new CanvasTransform(new Vec2(10, 120), Vec3.Zero, Vec2.One, new Vec2(40, 20), Vec2.Zero, Vec2.Zero, 0));
            _uiLayout.Set(new Sprite(_charactersTexture.Id, new RectInt(0, 0, 32, 32), Color32.White));
            Scene.Entities.CreateEntity(_uiLayout);
        }

        public override void OnFixedUpdate()
        {
            var total = Time.FixedTotal;
            Scene.Entities.ParallelForEach((ref Velocity velocity) =>
            {
                var timeStep = total - velocity.Time;
                velocity.Position += velocity.Vector * timeStep * SpeedScalar;
                velocity.Time = total;
            });
        }

        public override void OnRegisterEvents()
        {
            RegisterEvent<Velocity>(Event.OnAdd, OnAdd);
        }

        public override void OnLateUpdate()
        {
            if (Scene == null) return;

            var total = Time.Total;
            var delta = Time.UpdateDelta;
            Scene.Entities.ParallelForEach((ref Transform transform, ref Velocity velocity) =>
            {
                var timeStep = total - velocity.Time;
                transform.Position = velocity.Position + velocity.Vector * timeStep * SpeedScalar;
                if (velocity.Vector.X > 0 && transform.Position.X > 100) velocity.Vector.X *= -1;
                if (velocity.Vector.X < 0 && transform.Position.X < -100) velocity.Vector.X *= -1;
                if (velocity.Vector.Y > 0 && transform.Position.Y > 100) velocity.Vector.Y *= -1;
                if (velocity.Vector.Y < 0 && transform.Position.Y < -100) velocity.Vector.Y *= -1;
            });

            var fps = (int)MathF.Round(1f / Time.UpdateDelta);
            if (_fps != fps)
            {
                _fps = fps;
                UpdateTitle();
            }
        }

        private void OnAdd(uint entityId, ref Velocity velocity)
        {
            if (Scene == null) return;
            ref var transform = ref Scene.Entities.TryGetComponent<Transform>(entityId, out var found);
            if (!found) return;
            velocity.Position = (Vec2)transform.Position;
            velocity.Time = Time.Total;
        }

        private void UpdateTitle()
        {
            Window.Title = $"Pixl Demo - {Application.GraphicsApi} - {Scene.Entities.Count} - {_fps}";
        }
    }
}
