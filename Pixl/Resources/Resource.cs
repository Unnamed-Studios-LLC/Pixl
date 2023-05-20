namespace Pixl;

public abstract class Resource : IDisposable
{
    private bool _disposed;

    public uint Id { get; internal set; }

    internal Resources? Resources { get; set; }

    ~Resource() => DoDispose();

    public void Dispose()
    {
        DoDispose();
        GC.SuppressFinalize(this);
    }

    internal virtual void OnAdd() { }
    internal virtual void OnRemove() { }

    private void DoDispose()
    {
        if (_disposed) return;

        Resources?.Remove(this);
        Resources = null;

        _disposed = true;
    }
}
