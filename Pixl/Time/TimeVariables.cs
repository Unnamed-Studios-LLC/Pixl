namespace Pixl;

public struct TimeVariables
{
    /// <summary>
    /// Time variables in exact tick form, used for high precision 
    /// </summary>
    public PreciseVariables Precise;

    /// <summary>
    /// The total amount of time that has passed since application start.
    /// </summary>
    public float FixedTotal { get; internal set; }

    /// <summary>
    /// The amount of time that passes between Fixed Updates.
    /// </summary>
    public float FixedDelta { get; internal set; }

    /// <summary>
    /// The minimum amount of time that should elapse between FixedUpdates.
    /// </summary>
    public float TargetFixedDelta
    {
        get => Precise.TargetDelta / (float)PreciseVariables.TicksPerSecond;
        set => Precise.TargetDelta = (long)MathF.Floor(value * PreciseVariables.TicksPerSecond);
    }

    /// <summary>
    /// The minimum amount of time that should elapse between Updates.
    /// </summary>
    public float TargetUpdateDelta
    {
        get => Precise.TargetDelta / (float)PreciseVariables.TicksPerSecond;
        set => Precise.TargetDelta = (long)MathF.Floor(value * PreciseVariables.TicksPerSecond);
    }

    /// <summary>
    /// The total amount of time that has passed since application start.
    /// </summary>
    public float Total { get; internal set; }

    /// <summary>
    /// The amount of time that passed between Updates.
    /// </summary>
    public float Delta { get; internal set; }
}
