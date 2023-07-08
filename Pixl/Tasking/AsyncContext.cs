using System.Diagnostics;
using System.Threading;

namespace Pixl;

internal sealed class AsyncContext : SynchronizationContext
{
    public readonly struct SyncScope : IDisposable
    {
        private readonly SynchronizationContext? _context;

        public SyncScope(SynchronizationContext? context)
        {
            _context = context;
        }

        public void Dispose()
        {
            SetSynchronizationContext(_context);
        }
    }

    private readonly struct ActionRequest
    {
        private readonly SendOrPostCallback _callback;
        private readonly object? _state;
        private readonly ManualResetEvent? _waitEvent;

        public ActionRequest(SendOrPostCallback callback, object? state, ManualResetEvent? waitEvent = null)
        {
            _callback = callback;
            _state = state;
            _waitEvent = waitEvent;
        }

        public void Invoke()
        {
            try
            {
                _callback(_state);
            }
            finally
            {
                _waitEvent?.Set();
            }
        }
    }

    private const int InitialCapacity = 20;

    private readonly List<ActionRequest> _pendingActions = new(InitialCapacity);
    private readonly List<ActionRequest> _actionsToExecute = new(InitialCapacity);
    private readonly int _mainThreadId;
    private int _count = 0;

    public AsyncContext(int mainThreadId)
    {
        _pendingActions = new List<ActionRequest>(InitialCapacity);
        _mainThreadId = mainThreadId;
    }

    private AsyncContext(List<ActionRequest> pendingActions, int mainThreadId)
    {
        _pendingActions.AddRange(pendingActions);
        _mainThreadId = mainThreadId;
    }

    public void Close(TimeSpan timeout)
    {
        WaitForPending(timeout);
        SetSynchronizationContext(null);
    }

    public SyncScope CreateScope()
    {
        var current = Current;
        SetSynchronizationContext(this);
        return new SyncScope(current);
    }

    public override SynchronizationContext CreateCopy()
    {
        return new AsyncContext(_pendingActions, _mainThreadId);
    }

    public override void OperationStarted()
    {
        Interlocked.Increment(ref _count);
    }

    public override void OperationCompleted()
    {
        Interlocked.Decrement(ref _count);
    }

    public override void Post(SendOrPostCallback callback, object? state)
    {
        lock (_pendingActions)
        {
            _pendingActions.Add(new ActionRequest(callback, state));
        }
    }

    public void Run() => Execute();

    public override void Send(SendOrPostCallback callback, object? state)
    {
        if (_mainThreadId == Environment.CurrentManagedThreadId)
        {
            callback(state);
        }
        else
        {
            using var waitHandle = new ManualResetEvent(false);
            lock (_pendingActions)
            {
                _pendingActions.Add(new ActionRequest(callback, state, waitHandle));
            }
            waitHandle.WaitOne();
        }
    }

    private bool WaitForPending(TimeSpan timeout)
    {
        var time = Stopwatch.GetTimestamp();
        var waitEvent = new ManualResetEvent(false);

        while (HasPendingTasks())
        {
            var elapsed = Stopwatch.GetElapsedTime(time);
            if (elapsed > timeout) break;

            Execute();
            waitEvent.WaitOne(1);
        }

        return !HasPendingTasks();
    }

    private void Execute()
    {
        lock (_pendingActions)
        {
            _actionsToExecute.AddRange(_pendingActions);
            _pendingActions.Clear();
        }

        while (_actionsToExecute.Count > 0)
        {
            var action = _actionsToExecute[0];
            _actionsToExecute.RemoveAt(0);
            action.Invoke();
        }
    }

    private bool HasPendingTasks()
    {
        return _pendingActions.Count != 0 ||
            _count != 0;
    }
}
