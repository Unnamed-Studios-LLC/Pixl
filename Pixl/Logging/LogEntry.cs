namespace Pixl;

internal readonly struct LogEntry
{
    public LogEntry(uint id, DateTime timestamp, string message, string stacktrace, Exception? exception)
    {
        Id = id;
        Timestamp = timestamp;
        Message = message;
        Stacktrace = stacktrace;
        Exception = exception;
    }

    public uint Id { get; }
    public DateTime Timestamp { get; }
    public string Message { get; }
    public string Stacktrace { get; }
    public Exception? Exception { get; }
}
