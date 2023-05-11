namespace Pixl;

public sealed class CanvasSystem : ComponentSystem
{
    public CanvasSystem()
    {
        Order = 1001;
    }

    internal override void OnRender(VertexRenderer<Vertex> renderer)
    {
        renderer.ClearDepth();
        renderer.BeginBatch(Game.Current.Resources.DefaultMaterial.Id);

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

            var viewToClip = projectionMatrix * Matrix4x4.Scale(new Vec3(canvas.Scale.X, canvas.Scale.Y, 1)) * Matrix4x4.Translate(new Vec3(0, 0, -1));
            var parentSize = (Vec2)windowSize;

            Scene.Entities.ForEach((ref Sprite sprite, ref CanvasTransform transform) =>
            {
                transform.GetTransformationMatrix(out var modelToClip, in parentSize);
                modelToClip = viewToClip * modelToClip;

                // clockwise quad vertices
                Vec3 min = Vec3.Zero;
                Vec3 max = transform.Size;

                var a = new Vertex((Vec3)(modelToClip * min), Color.Blue);
                var b = new Vertex((Vec3)(modelToClip * new Vec2(min.X, max.Y)), Color.Red);
                var c = new Vertex((Vec3)(modelToClip * max), Color.Green);
                var d = new Vertex((Vec3)(modelToClip * new Vec2(max.X, min.Y)), Color.Yellow);

                renderer.RenderQuad(in a, in b, in c, in d);
            });
        });
    }
}
