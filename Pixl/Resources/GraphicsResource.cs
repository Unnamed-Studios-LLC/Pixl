namespace Pixl;

public abstract class GraphicsResource : Resource
{
    internal Graphics? Graphics { get; set; }

    internal void Create()
    {
        var graphics = Game.Current.Graphics;
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

        Create();
    }

    internal override void OnRemove()
    {
        base.OnRemove();

        Destroy();
    }

    internal virtual void OnCreate(Graphics graphics) { }
    internal virtual void OnDestroy(Graphics graphics) { }
}
