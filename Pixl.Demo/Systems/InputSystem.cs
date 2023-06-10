namespace Pixl.Demo.Systems
{
    internal class InputSystem : ComponentSystem
    {
        private VelocitySystem? _velocitySystem;
        private float _addTime;
        private float _removeTime;

        public override void OnAdd()
        {
            base.OnAdd();

            _velocitySystem = Scene?.GetSystem<VelocitySystem>();
        }

        public override void OnUpdate()
        {
            if (_velocitySystem == null ||
                _velocitySystem.CameraEntityId == 0) return;

            ref var cameraTransform = ref Scene.Entities.GetComponent<Transform>(_velocitySystem.CameraEntityId);
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

            var zoomSpeed = 1 + Time.UpdateDelta;
            if (Input.GetKey(KeyCode.Q))
            {
                cameraTransform.Scale *= zoomSpeed;
            }
            if (Input.GetKey(KeyCode.E))
            {
                cameraTransform.Scale *= 1 / zoomSpeed;// Vec3.Max(Vec3.Zero, cameraTransform.Scale - zoomSpeed);
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

            if (Input.GetKey(KeyCode.M))
            {
                _addTime += Time.UpdateDelta;
                while (_addTime >= 0.01f)
                {
                    _velocitySystem.CreateEntities(10);
                    _addTime -= 0.01f;
                }
            }

            if (Input.GetKey(KeyCode.N))
            {
                _removeTime += Time.UpdateDelta;
                while (_removeTime >= 0.01f)
                {
                    _velocitySystem.RemoveEntities(10);
                    _removeTime -= 0.01f;
                }
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                Application.GraphicsApi = Application.GraphicsApi == GraphicsApi.DirectX ? GraphicsApi.Vulkan : GraphicsApi.DirectX;
            }
        }
    }
}
