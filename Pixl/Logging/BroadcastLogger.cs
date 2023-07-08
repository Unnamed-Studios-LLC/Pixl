namespace Pixl;

internal sealed class BroadcastLogger : Logger
{
    private readonly List<Logger> _loggers = new();

    public BroadcastLogger(params Logger[] loggers)
    {
        if (loggers is null)
        {
            throw new ArgumentNullException(nameof(loggers));
        }

        _loggers.AddRange(loggers);
    }

    public override void Flush()
    {
        foreach (var logger in _loggers)
        {
            logger.Flush();
        }
    }

    public override void Log(object @object)
    {
        foreach (var logger in _loggers)
        {
            logger.Log(@object);
        }
    }
}
