namespace Pixl;

internal class DiagnosticsLogger : ILogger
{
    public void Flush()
    {

    }

    public void Log(object @object)
    {
        System.Diagnostics.Debug.WriteLine(@object);
    }
}
