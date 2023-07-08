using System.Threading;
using System.Threading.Tasks;

namespace Pixl.Editor;

internal class EditorTask
{
    public EditorTask(string name, bool cancellable = false)
    {
        Name = name;
        CancellationToken = cancellable ? new() : null;
    }

    public string Name { get; }
    public Task? Task { get; private set; } = null;
    public string? State { get; set; } = null;
    public float? Progress { get; set; } = null;
    public CancellationTokenSource? CancellationToken { get; }
    public Action? OnComplete { get; private set; } = null;

    public virtual void Complete()
    {
        if (Task != null && Task.IsCompletedSuccessfully) OnComplete?.Invoke();
        CancellationToken?.Dispose();
    }

    public void SetTask(Task task, Action? onComplete = null)
    {
        Task = task ?? throw new ArgumentNullException(nameof(task));
        OnComplete = onComplete;
    }

    public void SetTask<T>(Task<T> task, Action<T>? onComplete = null)
    {
        SetTask(task, () =>
        {
            onComplete?.Invoke(task.Result);
        });
    }
}