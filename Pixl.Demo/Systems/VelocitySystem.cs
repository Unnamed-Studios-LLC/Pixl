using EntitiesDb;
using Pixl.Demo.Components;

namespace Pixl.Demo.Systems
{
    internal sealed class VelocitySystem : ComponentSystem
    {
        [EntityId]
        public uint CameraEntityId;
        public float Speed = 1;

        public sbyte Sbyte = 1;
        public short Short = 1;
        public int Int = 1;
        public long Long = 1;

        public byte Byte = 1;
        public ushort Ushort = 1;
        public uint Uint = 1;
        public ulong Ulong = 1;

        public float Float = 1;
        public double Double = 1;

        public Color32 Color = Color32.Red;

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
                .Add<Named>()
                .Build();

            _canvasLayout = EntityLayoutBuilder.Create()
                .Add<Canvas>()
                .Add<Named>()
                .Build();

            _entityLayout = EntityLayoutBuilder.Create()
                .Add<Transform>()
                .Add<Sprite>()
                .Add<Velocity>()
                .Add<Named>()
                .Build();

            _uiLayout = EntityLayoutBuilder.Create()
                .Add<CanvasTransform>()
                .Add<Sprite>()
                .Add<Named>()
                .Build();

            var path = Path.Combine(App.AssetsPath, "characters.png");
            var fileStream = File.OpenRead(path);
            _charactersTexture = Texture2d.CreateFromFile(fileStream, SampleMode.Point, ColorFormat.Rgba32);
        }

        public void CreateEntities(int count)
        {
            var rnd = new Random();
            for (int i = 0; i < count; i++)
            {
                var heading = (float)rnd.NextDouble() * MathF.PI * 2;
                var speed = (float)rnd.NextDouble() * 5;
                var vector = new Vec2(MathF.Sin(heading), MathF.Cos(heading)) * speed;
                var position = new Vec2(-100 + (float)rnd.NextDouble() * 200, -100 + (float)rnd.NextDouble() * 200);
                Vec2 scale = (0.5f + (float)rnd.NextDouble()) * 0.75f;
                var color = new Color32((byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256), 255);

                _entityLayout.Set(new Transform(position, Vec3.Zero, new Vec3(scale.X, scale.Y, 0)));
                _entityLayout.Set(new Sprite(_charactersTexture.Id, new RectInt(0, 0, 8, 8), color));
                _entityLayout.Set(new Velocity(vector));
                var entityId = Scene.Entities.CreateEntity(_entityLayout);
                _entityIds.Add(entityId);
            }
            UpdateTitle();

            Debug.Log($"Created {count} entities");
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

            Debug.Log($"Removed {count} entities");
        }

        public override void OnAdd()
        {
            var nameSystem = Scene.GetSystem<NameSystem>();

            // create camera
            _cameraLayout.Set(new Transform(new Vec3(0, 0, 10), new Vec3(0, 0, 0), Vec3.One));
            _cameraLayout.Set(new Camera());
            CameraEntityId = Scene.Entities.CreateEntity(_cameraLayout);
            nameSystem?.SetName(CameraEntityId, "Camera");

            // create canvas
            _canvasLayout.Set(new Canvas(new Vec2(1, 1)));
            var entityId = Scene.Entities.CreateEntity(_canvasLayout);
            nameSystem?.SetName(entityId, "Canvas");

            // create ui
            _uiLayout.Set(new CanvasTransform(new Vec2(10, 10), Vec3.Zero, Vec2.One, new Vec2(100, 100), Vec2.Zero, Vec2.Zero, 0));
            _uiLayout.Set(new Sprite(_charactersTexture.Id, new RectInt(0, 0, 32, 32), Color32.White));
            entityId = Scene.Entities.CreateEntity(_uiLayout);
            nameSystem?.SetName(entityId, "UI Element 1");

            _uiLayout.Set(new CanvasTransform(new Vec2(120, 10), Vec3.Zero, Vec2.One, new Vec2(20, 20), Vec2.Zero, Vec2.Zero, 0));
            _uiLayout.Set(new Sprite(0, new RectInt(0, 0, 32, 32), Color32.White));
            entityId = Scene.Entities.CreateEntity(_uiLayout);
            nameSystem?.SetName(entityId, "UI Element 2");

            _uiLayout.Set(new CanvasTransform(new Vec2(10, 120), Vec3.Zero, Vec2.One, new Vec2(40, 20), Vec2.Zero, Vec2.Zero, 0));
            _uiLayout.Set(new Sprite(_charactersTexture.Id, new RectInt(0, 0, 32, 32), Color32.White));
            entityId = Scene.Entities.CreateEntity(_uiLayout);
            nameSystem?.SetName(entityId, "UI Element 3");

            CreateEntities(5);
        }

        public override void OnLateUpdate()
        {
            if (Scene == null) return;

            var total = TimeVariables.Total;
            var delta = TimeVariables.Delta;
            Scene.Entities.ParallelForEach((ref Transform transform, ref Velocity velocity) =>
            {
                transform.Position += velocity.Vector * delta * Speed;
                if (velocity.Vector.X > 0 && transform.Position.X > 100) velocity.Vector.X *= -1;
                if (velocity.Vector.X < 0 && transform.Position.X < -100) velocity.Vector.X *= -1;
                if (velocity.Vector.Y > 0 && transform.Position.Y > 100) velocity.Vector.Y *= -1;
                if (velocity.Vector.Y < 0 && transform.Position.Y < -100) velocity.Vector.Y *= -1;
            });

            var fps = (int)MathF.Round(1f / TimeVariables.Delta);
            if (_fps != fps)
            {
                _fps = fps;
                UpdateTitle();
            }
        }

        private void UpdateTitle()
        {
            var windowSize = Screen.Size;
            Screen.Title = $"Pixl Demo - {App.GraphicsApi} - {Scene.Entities.Count} - {_fps} - {windowSize}";
        }
    }
}
