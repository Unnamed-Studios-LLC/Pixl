using System.Threading.Tasks;

namespace Pixl;

internal sealed class FileLogger : ILogger
{
    private readonly string _outputFolder;
    private readonly bool _immediateWrite;
    private readonly string _outputFilePath;

    private readonly object _queueLock = new();
    private readonly Stack<Queue<string>> _queueCache = new();
    private Queue<string> _writeQueue = new();
    private readonly string[] _single = new string[1];

    public FileLogger(string outputFolder, string fileName, bool immediateWrite)
    {
        if (fileName is null)
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        _outputFolder = outputFolder ?? throw new ArgumentNullException(nameof(outputFolder));
        _immediateWrite = immediateWrite;
        _outputFilePath = Path.Combine(outputFolder, $"{fileName}.log");
        if (File.Exists(_outputFilePath))
        {
            var oldFilePath = Path.Combine(outputFolder, $"{fileName}-old.log");
            File.Move(_outputFilePath, oldFilePath, true);
        }
    }

    public void Flush()
    {
        lock (_queueLock)
        {
            if (_writeQueue.Count == 0) return;
            FlushCore(_writeQueue);
        }
    }

    public void Log(object @object)
    {
        var @string = FormatObject(@object);
        if (_immediateWrite) WriteImmediate(@string);
        else EnqueueWrite(@string);
    }

    internal static string FormatObject(object @object)
    {
        var @string = @object?.ToString() ?? "null";
        var timestamp = DateTime.Now.ToLongTimeString();
        return $"[{timestamp}] {@string}";
    }

    private void EnqueueWrite(string @string)
    {
        lock (_queueLock)
        {
            _writeQueue.Enqueue(@string);
            if (_writeQueue.Count > 20)
            {
                FlushCore(_writeQueue);
                _writeQueue = _queueCache.TryPop(out var cachedQueue) ? cachedQueue : new();
            }
        }
    }

    private void EnsureDirectory()
    {
        Directory.CreateDirectory(_outputFolder);
    }

    private void FlushCore(Queue<string> queue)
    {
        Task.Run(() =>
        {
            EnsureDirectory();
            File.AppendAllLines(_outputFilePath, queue);
            lock (_queueLock)
            {
                queue.Clear();
                _queueCache.Push(queue);
            }
        });
    }

    private void WriteImmediate(string @string)
    {
        EnsureDirectory();
        _single[0] = @string;
        File.AppendAllLines(_outputFilePath, _single);
    }
}
