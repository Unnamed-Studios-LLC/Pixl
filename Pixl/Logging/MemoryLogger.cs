namespace Pixl;

internal class MemoryLogger : ILogger
{
    private readonly List<string> _logs = new();
    private readonly int _maxByteSize;
    private readonly int _cutdownSize;
    private int _byteSize;

    public MemoryLogger(int maxByteSize, int cutdownSize)
    {
        _maxByteSize = maxByteSize;
        _cutdownSize = cutdownSize;
    }

    public IReadOnlyList<string> Logs => _logs;

    public void Flush()
    {
        // no flush needed for memory logging
    }

    public void Log(object @object)
    {
        var formatted = FileLogger.FormatObject(@object);
        var sizeOfFormatted = SizeOfLog(formatted);
        lock (_logs)
        {
            // make space (removing up to _cutdownSize if we can)
            MakeSpace(sizeOfFormatted, _cutdownSize);

            // append log
            _logs.Add(formatted);
        }
    }

    private static int SizeOfLog(string log)
    {
        // rough size (string has some other bytes to it, works well enough for our purpose)
        return log.Length * sizeof(char);
    }

    private void MakeSpace(int minSpace, int maxSpace)
    {
        if (minSpace >= _maxByteSize) throw new Exception("Unable to make space for, requested min byte size is larger than the max byte size!");
        var spaceToFree = Math.Max(minSpace, Math.Min(maxSpace, _maxByteSize));

        // determine how many logs to remove
        int removeCount = 0;
        var postRemoveByteSize = _byteSize;
        while (postRemoveByteSize + spaceToFree > _maxByteSize)
        {
            var log = _logs[removeCount++];
            var size = SizeOfLog(log);
            postRemoveByteSize -= size;
        }

        // remove from log list
        if (removeCount > 0)
        {
            _logs.RemoveRange(0, removeCount);
            _byteSize = postRemoveByteSize;
        }
    }
}
