using EntitiesDb;
using Pixl.Demo.Components;
using static System.Formats.Asn1.AsnWriter;
using System.Numerics;

namespace Pixl.Demo.Systems
{
    internal sealed class VelocitySystem : ComponentSystem
    {
        private uint _cameraEntityId;
        private readonly EntityLayout _cameraLayout;
        private readonly EntityLayout _canvasLayout;
        private readonly EntityLayout _entityLayout;
        private readonly EntityLayout _uiLayout;
        private int _entityCount = 0;

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
        }
        public override void OnAdd()
        {
            // create camera
            _cameraLayout.Set(new Transform(new Vec3(0, 0, 10), new Vec3(0, 0, 0), Vec3.One));
            _cameraLayout.Set(new Camera());
            _cameraEntityId = Scene.Entities.CreateEntity(_cameraLayout);

            // create canvas
            _canvasLayout.Set(new Canvas(new Vec2(1, 1)));
            Scene.Entities.CreateEntity(_canvasLayout);

            // create entities
            CreateEntities(30_000);

            // create ui
            _uiLayout.Set(new CanvasTransform(new Vec2(10, 10), Vec3.Zero, Vec2.One, new Vec2(100, 100), Vec2.Zero, Vec2.Zero, 0));
            _uiLayout.Set(new Sprite(0, new RectInt(0, 0, 20, 20)));
            Scene.Entities.CreateEntity(_uiLayout);

            _uiLayout.Set(new CanvasTransform(new Vec2(120, 10), Vec3.Zero, Vec2.One, new Vec2(20, 20), Vec2.Zero, Vec2.Zero, 0));
            _uiLayout.Set(new Sprite(0, new RectInt(0, 0, 20, 20)));
            Scene.Entities.CreateEntity(_uiLayout);

            _uiLayout.Set(new CanvasTransform(new Vec2(10, 120), Vec3.Zero, Vec2.One, new Vec2(40, 20), Vec2.Zero, Vec2.Zero, 0));
            _uiLayout.Set(new Sprite(0, new RectInt(0, 0, 20, 20)));
            Scene.Entities.CreateEntity(_uiLayout);
        }

        public override void OnFixedUpdate()
        {
            var total = Time.FixedTotal;
            Scene.Entities.ParallelForEach((ref Velocity velocity) =>
            {
                var timeStep = total - velocity.Time;
                velocity.Position += velocity.Vector * timeStep;
                velocity.Time = total;
            });
        }

        public override void OnRegisterEvents()
        {
            RegisterEvent<Velocity>(Event.OnAdd, OnAdd);
        }

        public override void OnUpdate()
        {
            if (Scene == null) return;
            if (_cameraEntityId != 0)
            {
                ref var cameraTransform = ref Scene.Entities.GetComponent<Transform>(_cameraEntityId);
                var moveSpeed = 20 * Time.UpdateDelta;
                if (Input.GetKey(KeyCode.A))
                {
                    cameraTransform.Position.X -= moveSpeed;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    cameraTransform.Position.X += moveSpeed;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    cameraTransform.Position.Y -= moveSpeed;
                }
                if (Input.GetKey(KeyCode.W))
                {
                    cameraTransform.Position.Y += moveSpeed;
                }

                if (Input.GetKey(KeyCode.Q))
                {
                    cameraTransform.Scale *= 1.2f;
                }
                if (Input.GetKey(KeyCode.E))
                {
                    cameraTransform.Scale /= 1.2f;
                }

                var rotateSpeed = 180 * Time.UpdateDelta;
                if (Input.GetKey(KeyCode.C))
                {
                    cameraTransform.Rotation.Z += rotateSpeed;
                }
                if (Input.GetKey(KeyCode.V))
                {
                    cameraTransform.Rotation.Z -= rotateSpeed;
                }

                if (Input.GetKey(KeyCode.R))
                {
                    cameraTransform.Position = Vec3.Zero;
                }

                if (Input.GetKeyDown(KeyCode.M))
                {
                    CreateEntities(10_000);
                }

                if (Input.GetKeyDown(KeyCode.G))
                {
                    Application.GraphicsApi = Application.GraphicsApi == GraphicsApi.DirectX ? GraphicsApi.Vulkan : GraphicsApi.DirectX;
                }
            }

            var total = Time.Total;
            var delta = Time.UpdateDelta;
            Scene.Entities.ParallelForEach((ref Transform transform, ref Velocity velocity) =>
            {
                var timeStep = total - velocity.Time;
                transform.Position = velocity.Position + velocity.Vector * timeStep;
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

        private void CreateEntities(int count)
        {
            var rnd = new Random();
            for (int i = 0; i < count; i++)
            {
                var heading = (float)rnd.NextDouble() * MathF.PI * 2;
                var speed = (float)rnd.NextDouble() * 5;
                var vector = new Vec2(MathF.Sin(heading), MathF.Cos(heading)) * speed;
                var position = new Vec2(-100 + (float)rnd.NextDouble() * 200, -100 + (float)rnd.NextDouble() * 200);
                Vec2 scale = (0.5f + (float)rnd.NextDouble()) * 0.3f;

                _entityLayout.Set(new Transform(position, Vec3.Zero, scale));
                _entityLayout.Set(new Sprite(0, new RectInt(0, 0, 1, 1)));
                _entityLayout.Set(new Velocity(vector));
                Scene.Entities.CreateEntity(_entityLayout);
            }
            _entityCount += count;
            UpdateTitle();
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
