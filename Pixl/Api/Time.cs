namespace Pixl;

public static class Time
{
    /// <summary>
    /// <inheritdoc cref="TimeVariables.FixedTotal"/>
    /// </summary>
    public static float FixedTotal => Variables.FixedTotal;

    /// <summary>
    /// <inheritdoc cref="TimeVariables.FixedDelta"/>
    /// </summary>
    public static float FixedDelta => Variables.FixedDelta;

    /// <summary>
    /// <inheritdoc cref="TimeVariables.TargetFixedDelta"/>
    /// </summary>
    public static float TargetFixedDelta
    {
        get => Precise.TargetDelta / (float)PreciseVariables.TicksPerSecond;
        set => Precise.TargetDelta = (long)MathF.Floor(value * PreciseVariables.TicksPerSecond);
    }

    /// <summary>
    /// <inheritdoc cref="TimeVariables.TargetUpdateDelta"/>
    /// </summary>
    public static float TargetUpdateDelta
    {
        get => Precise.TargetDelta / (float)PreciseVariables.TicksPerSecond;
        set => Precise.TargetDelta = (long)MathF.Floor(value * PreciseVariables.TicksPerSecond);
    }

    /// <summary>
    /// <inheritdoc cref="TimeVariables.Total"/>
    /// </summary>
    public static float Total => Variables.Total;

    /// <summary>
    /// <inheritdoc cref="TimeVariables.Delta"/>
    /// </summary>
    public static float Delta => Variables.Delta;

    internal static ref TimeVariables Variables => ref Game.Shared.Time;
}
