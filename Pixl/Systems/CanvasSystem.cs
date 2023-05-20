namespace Pixl;

public sealed class CanvasSystem : ComponentSystem
{
    public Material? Material;
    public SharedProperty? WorldToClipMatrix;

    public CanvasSystem()
    {
        Order = 1001;

        var resources = Game.Current.Resources;
        Material = resources.DefaultMaterial;
        WorldToClipMatrix = resources.WorldToClipMatrix;
    }

    public CanvasSystem(Material? material, SharedProperty? worldToClipMatrix) : this()
    {
        Material = material;
        WorldToClipMatrix = worldToClipMatrix;
    }

    internal override void OnRender(VertexRenderer renderer)
    {
        renderer.ClearDepth();
        Scene.Entities.ForEach((ref Canvas canvas) =>
        {
            var windowSize = (Int2)(Window.Size / canvas.Scale);
            var remainder = new Int2(windowSize.X % 2, windowSize.Y % 2);
            var half = windowSize / 2;

            var projectionMatrix = Matrix4x4.Orthographic(
                0,
                windowSize.X,
                0,
                windowSize.Y,
                -1000, 1000);

            var parentSize = (Vec2)windowSize;
            var worldToClip = projectionMatrix * Matrix4x4.Scale(new Vec3(canvas.Scale.X, canvas.Scale.Y, 1)) * Matrix4x4.Translate(new Vec3(0, 0, -1));
            WorldToClipMatrix?.Set(worldToClip);

            renderer.BeginBatch(Material?.Id ?? 0);
            Scene.Entities.ForEach((ref Sprite sprite, ref CanvasTransform transform) =>
            {
                transform.GetTransformationMatrix(out var modelToWorld, in parentSize);

                // clockwise quad vertices
                Vec3 min = Vec3.Zero;
                Vec3 max = transform.Size;

                var a = new PositionColorVertex((Vec3)(modelToWorld * min), Color.Blue);
                var b = new PositionColorVertex((Vec3)(modelToWorld * new Vec2(min.X, max.Y)), Color.Red);
                var c = new PositionColorVertex((Vec3)(modelToWorld * max), Color.Green);
                var d = new PositionColorVertex((Vec3)(modelToWorld * new Vec2(max.X, min.Y)), Color.Yellow);

                renderer.RenderQuad(in a, in b, in c, in d);
            });
        });
    }
}
