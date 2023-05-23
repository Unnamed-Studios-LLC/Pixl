namespace Pixl;

internal abstract class Api
{
    private int? _mainThreadId;

    public Api(IPlayer player)
    {
        Player = player ?? throw new ArgumentNullException(nameof(player));
    }

    public InputState Input { get; } = new();
    public IPlayer Player { get; }
    public long Time { get; protected set; }

    // ================
    // ================  Application
    // ================

    /// <summary>
    /// The path pointing to application assets. May be read-only on certain platforms. Use <see cref="Application.DataPath"/> to save game data.
    /// </summary>
    public string AssetsPath => Player.AssetsPath;
    /// <summary>
    /// The path pointing to a directory where application data may be read and written.
    /// </summary>
    public string DataPath => Player.DataPath;
    /// <summary>
    /// Id of the Thread running the main game loop
    /// </summary>
    public int MainThreadId
    {
        get => _mainThreadId ?? throw new Exception($"{nameof(MainThreadId)} has not been initialized!");
        protected set => _mainThreadId = value;
    }

    // ================
    // ================  Debug
    // ================

    /// <summary>
    /// The path pointing to application assets. May be read-only on certain platforms. Use <see cref="Application.DataPath"/> to save game data.
    /// </summary>
    public void Log(object @object) => Player.Log(@object);

    // ================
    // ================  Input
    // ================

    public bool GetKey(KeyCode keyCode)
    {
        ref var record = ref Input.GetKeyRecord(keyCode);
        return record.State == KeyState.Pressed;
    }

    public bool GetKeyDown(KeyCode keyCode)
    {
        ref var record = ref Input.GetKeyRecord(keyCode);
        return record.State == KeyState.Pressed && record.Time == Time;
    }

    public bool GetKeyUp(KeyCode keyCode)
    {
        ref var record = ref Input.GetKeyRecord(keyCode);
        return record.State == KeyState.Released && record.Time == Time;
    }

    public bool GetMouseButton() => throw new NotImplementedException();

    public bool GetMouseButtonDown() => throw new NotImplementedException();

    public bool GetMouseButtonUp() => throw new NotImplementedException();

    // ================
    // ================  Time
    // ================

    /// <summary>
    /// Time variables in exact tick form, used for extreme precision 
    /// </summary>
    public PreciseTime Precise { get; } = new();

    /// <summary>
    /// The total amount of time that has passed since application start.
    /// </summary>
    public float FixedTimeTotal { get; internal set; } = 0;
    /// <summary>
    /// The amount of time that passes between Fixed Updates.
    /// </summary>
    public float FixedUpdateDelta
    {
        get => Precise.FixedUpdateDelta / (float)PreciseTime.TicksPerSecond;
        set => Precise.FixedUpdateDelta = (long)MathF.Floor(value * PreciseTime.TicksPerSecond);
    }
    /// <summary>
    /// The minimum amount of time that should elapse between Updates.
    /// </summary>
    public float TargetUpdateDelta
    {
        get => Precise.TargetUpdateDelta / (float)PreciseTime.TicksPerSecond;
        set => Precise.TargetUpdateDelta = (long)MathF.Floor(value * PreciseTime.TicksPerSecond);
    }
    /// <summary>
    /// The total amount of time that has passed since application start.
    /// </summary>
    public float TimeTotal { get; internal set; } = 0;
    /// <summary>
    /// The amount of time that passed between Updates.
    /// </summary>
    public float UpdateDelta { get; internal set; } = 0;
}
