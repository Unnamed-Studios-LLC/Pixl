namespace Pixl;

public sealed class TransformSystem : ComponentSystem
{
    public TransformSystem()
    {
        Order = 900;
    }

    internal override void OnRender(VertexRenderer renderer)
    {
        // parallel matrix multiplcation and hierarchy calculation
        Scene.Entities.ParallelForEach((uint entityId, ref Transform transform) =>
        {
            transform.LocalToWorld = Matrix4x4.Transformation(in transform.Position, in transform.Rotation, in transform.Scale);
        });
    }
}
