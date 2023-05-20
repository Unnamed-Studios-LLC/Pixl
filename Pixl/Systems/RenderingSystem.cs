using EntitiesDb;

namespace Pixl;

public sealed class RenderingSystem : ComponentSystem
{
    public Material? Material;
    public SharedProperty? WorldToClipMatrix;
    private bool _matrixFlag;

    public RenderingSystem()
    {
        Order = 1000;

        var resources = Game.Current.Resources;
        Material = resources.ErrorMaterial;
        WorldToClipMatrix = resources.WorldToClipMatrix;
    }

    public RenderingSystem(Material? material, SharedProperty? worldToClipMatrix) : this()
    {
        Material = material;
        WorldToClipMatrix = worldToClipMatrix;
    }

    public override void OnRegisterEvents()
    {
        RegisterEvent<Transform>(Event.OnAdd, OnAdd);
    }

    internal override void OnRender(VertexRenderer renderer)
    {
        // parallel matrix multiplcation and hierarchy calculation
        _matrixFlag = !_matrixFlag;
        Scene.Entities.ParallelForEach((ref Transform transform) =>
        {
            Scene.Entities.UpdateWorldMatrix(ref transform, _matrixFlag);
        });

        Scene.Entities.ForEach((ref Camera camera, ref Transform cameraTransform) =>
        {
            var windowSize = Window.Size;
            var remainder = new Int2(windowSize.X % 2, windowSize.Y % 2);
            var half = windowSize / 2;

            var projectionMatrix = Matrix4x4.Orthographic(
                -half.X,
                half.X + remainder.X,
                -half.Y,
                half.Y + remainder.Y,
                -1000 * cameraTransform.Scale.Z, 1000 * cameraTransform.Scale.Z);

            cameraTransform.GetViewMatrix(out var worldToClip);
            worldToClip = projectionMatrix * worldToClip;
            WorldToClipMatrix?.Set(worldToClip);

            renderer.BeginBatch(Material?.Id ?? 0);
            Scene.Entities.ForEach((ref Sprite sprite, ref Transform transform) =>
            {
                // clockwise quad vertices
                Vec3 min = sprite.Rect.Min;
                Vec3 max = sprite.Rect.Max;

                var a = new PositionColorVertex((Vec3)(transform.WorldMatrix * min), Color.Blue);
                var b = new PositionColorVertex((Vec3)(transform.WorldMatrix * new Vec2(min.X, max.Y)), Color.Red);
                var c = new PositionColorVertex((Vec3)(transform.WorldMatrix * max), Color.Green);
                var d = new PositionColorVertex((Vec3)(transform.WorldMatrix * new Vec2(max.X, min.Y)), Color.Yellow);

                renderer.RenderQuad(in a, in b, in c, in d);
            });
        });
    }

    private void OnAdd(uint entityId, ref Transform transform)
    {
        transform.Flag = _matrixFlag;
    }
}
