namespace Pixl;

internal sealed class Property : IDisposable
{
    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
            }

            _disposed = true;
        }
    }

    public void Dispose() => Dispose(true);
}
