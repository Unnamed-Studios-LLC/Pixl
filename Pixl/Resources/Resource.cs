namespace Pixl;

public abstract class Resource : IDisposable
{
    private bool _disposedValue;

    public uint Id { get; internal set; }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Game.Current.Resources.Remove(this);
            }

            _disposedValue = true;
        }
    }

    internal virtual void OnAdd() { }
    internal virtual void OnRemove() { }
}
