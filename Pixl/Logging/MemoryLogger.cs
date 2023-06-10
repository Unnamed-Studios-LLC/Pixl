namespace Pixl;

internal sealed class MemoryLogger : ILogger
{
    private readonly List<LogEntry> _logs = new();
    private readonly int _maxCount;
    private readonly int _cutdownCount;
    private uint _nextLogId;

    public MemoryLogger(int maxCount, int cutdownCount)
    {
        if (maxCount <= 0) throw new ArgumentOutOfRangeException(nameof(maxCount), "Parameter must be greater than 0");
        if (cutdownCount <= 0) throw new ArgumentOutOfRangeException(nameof(cutdownCount), "Parameter must be greater than 0");
        _maxCount = maxCount;
        _cutdownCount = cutdownCount;
    }

    public void Clear()
    {
        lock (_logs)
        {
            _logs.Clear();
        }
    }

    public void Flush()
    {
        // no flush needed for memory logging
    }

    public void Log(object @object)
    {
        var stacktrace = Environment.StackTrace;
        var formatted = FileLogger.FormatObject(@object);
        lock (_logs)
        {
            // make space
            if (_logs.Count >= _maxCount)
            {
                var toRemove = Math.Min(_logs.Count, _cutdownCount);
                _logs.RemoveRange(0, toRemove);
            }

            // append log
            var entry = new LogEntry(_nextLogId++, DateTime.Now, formatted, stacktrace, null);
            _logs.Add(entry);
        }
    }

    public void Read(Action<List<LogEntry>> action)
    {
        lock (_logs)
        {
            action(_logs);
        }
    }
}
