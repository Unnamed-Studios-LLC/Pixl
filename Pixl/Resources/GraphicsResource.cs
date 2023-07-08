namespace Pixl;

public abstract class GraphicsResource : Resource
{
    internal Graphics? Graphics { get; set; }

    internal void Create(Graphics graphics)
    {
        if (!graphics.Setup) return;
        if (Graphics != null) throw new Exception($"{nameof(GraphicsResource)} already created!");
        Graphics = graphics;
        OnCreate(graphics);
    }

    internal void Destroy()
    {
        if (Graphics == null || !Graphics.Setup) return;
        OnDestroy(Graphics);
        Graphics = null;
    }

    internal override void OnAdd()
    {
        base.OnAdd();

        var graphics = Resources!.Graphics;
        Create(graphics);
    }

    internal override void OnRemove()
    {
        base.OnRemove();

        Destroy();
    }

    internal virtual void OnCreate(Graphics graphics) { }
    internal virtual void OnDestroy(Graphics graphics) { }
}
