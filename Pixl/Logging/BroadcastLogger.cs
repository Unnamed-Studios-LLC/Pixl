namespace Pixl;

internal sealed class BroadcastLogger : ILogger
{
    private readonly List<ILogger> _loggers = new();

    public BroadcastLogger(params ILogger[] loggers)
    {
        if (loggers is null)
        {
            throw new ArgumentNullException(nameof(loggers));
        }

        _loggers.AddRange(loggers);
    }

    public void Flush()
    {
        foreach (var logger in _loggers)
        {
            logger.Flush();
        }
    }

    public void Log(object @object)
    {
        foreach (var logger in _loggers)
        {
            logger.Log(@object);
        }
    }
}
