namespace Pixl;

public abstract class GraphicsResource : Resource
{
    private bool _created = false;

    internal void Create()
    {
        if (!Game.Current.Graphics.Setup) return;
        if (_created) throw new Exception($"{nameof(GraphicsResource)} already created!");
        OnCreate();
        _created = true;
    }

    internal void Destroy()
    {
        if (!_created || !Game.Current.Graphics.Setup) return;
        OnDestroy();
        _created = false;
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

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            Destroy();
        }
    }

    protected virtual void OnCreate() { }
    protected virtual void OnDestroy() { }
}
