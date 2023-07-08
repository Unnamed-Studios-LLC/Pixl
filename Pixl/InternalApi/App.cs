using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Pixl;

internal abstract class App
{
    public TimeVariables Time;

    private readonly ManualResetEvent _waitEvent = new(false);
    private AsyncContext? _asyncContext;
    private long _startTime;
    private long _time;

    public App(Window window, Graphics graphics, Resources resources, Values values, Logger logger, Files files)
    {
        Window = window;
        Graphics = graphics;
        Resources = resources;
        Values = values;
        Logger = logger;
        Files = files;
    }
    
    public Window Window { get; }
    public Graphics Graphics { get; }
    public Resources Resources { get; }
    public Values Values { get; }
    public Logger Logger { get; }
    public Files Files { get; }

    public int ExitCode { get; set; }

    /// <summary>
    /// The graphics api currently being used
    /// </summary>
    public GraphicsApi GraphicsApi
    {
        get => Graphics.Api;
        set => SetGraphicsApi(value);
    }

    /// <summary>
    /// Id of the <see cref="Thread"/> running the main loop
    /// </summary>
    public int MainThreadId { get; private set; }

    /// <summary>
    /// If execution is currently on the main thread
    /// </summary>
    public bool OnMainThread => Environment.CurrentManagedThreadId == MainThreadId;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CallUserMethod(Action action)
    {
        try
        {
            action?.Invoke();
        }
        catch (Exception e)
        {
            Logger.Log(e);
        }
    }

    public virtual bool ProcessEvent(ref WindowEvent @event)
    {
        return @event.Type switch
        {
            WindowEventType.Quit => false,
            _ => true,
        };
    }

    public virtual bool ProcessEvents(Span<WindowEvent> events)
    {
        foreach (ref var @event in events)
        {
            if (!ProcessEvent(ref @event)) return false;
        }
        return true;
    }

    /// <summary>
    /// Must be called from the main thread. See <see cref="App.OnMainThread"/>.
    /// </summary>
    public void RequireMainThread([CallerMemberName] string? callerName = null)
    {
        if (!OnMainThread) throw new Exception($"{callerName} must be called on the main thread");
    }

    public bool Run()
    {
        using var asyncScope = _asyncContext?.CreateScope();

        UpdateTime();
        var events = Window.DequeueEvents();
        if (!ProcessEvents(events)) return false;
        _asyncContext?.Run();
        Update();
        Render();
        Logger.Flush();
        return true;
    }

    public virtual void Start()
    {
        MainThreadId = Environment.CurrentManagedThreadId;
        _startTime = Stopwatch.GetTimestamp();
        Window.Start();
        Values.Start();
        _asyncContext = new AsyncContext(MainThreadId);
    }

    public virtual void Stop()
    {
        _asyncContext?.Close(TimeSpan.FromSeconds(2));
        Values.Stop();
        Window.Stop();
        Logger.Flush();
    }

    public abstract void Render();

    public abstract void Update();

    public void WaitForNextUpdate()
    {
        var ticks = GetTicksUntilNextUpdate();
        if (ticks <= 0) return;
        var spinWaitMaxTicks = 2 * Stopwatch.Frequency / 1000; // 2 ms
        var startTime = Stopwatch.GetTimestamp();

        if (ticks > spinWaitMaxTicks)
        {
            // wait most of the duration
            _waitEvent.WaitOne((int)((ticks - spinWaitMaxTicks) / (Stopwatch.Frequency / 1000)));
        }

        while (Stopwatch.GetTimestamp() - startTime < ticks) { } // spin last bits
    }

    private long GetTicksUntilNextUpdate()
    {
        var timestamp = Stopwatch.GetTimestamp();
        var currentTime = timestamp - _startTime;
        var targetDelta = Time.Precise.TargetDelta;
        var nextUpdateTime = Time.Precise.Total + targetDelta;
        return nextUpdateTime - currentTime;
    }

    private void SetGraphicsApi(GraphicsApi graphicsApi)
    {
        RequireMainThread();
        if (graphicsApi == Graphics.Api) return;
        Graphics.Stop(Resources);
        Graphics.Start(Resources, Window, graphicsApi);
    }

    private void UpdateTime()
    {
        var timestamp = Stopwatch.GetTimestamp();
        var newTime = timestamp - _startTime;
        var updateDelta = newTime - _time;
        _time = newTime;

        Time.Precise.Total = newTime;
        Time.Precise.Delta = updateDelta;
        Time.Total = newTime / (float)PreciseVariables.TicksPerSecond;
        Time.Delta = updateDelta / (float)PreciseVariables.TicksPerSecond;
    }
}
