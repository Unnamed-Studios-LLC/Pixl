namespace Pixl;

public static class Precise
{
    /// <summary>
    /// <inheritdoc cref="PreciseVariables.TicksPerSecond"/>
    /// </summary>
    public static long TicksPerSecond => PreciseVariables.TicksPerSecond;

    /// <summary>
    /// <inheritdoc cref="TimeVariables.FixedDelta"/>
    /// </summary>
    public static long FixedDelta => Variables.FixedDelta;

    /// <summary>
    /// <inheritdoc cref="TimeVariables.FixedTotal"/>
    /// </summary>
    public static long FixedTotal => Variables.FixedTotal;

    /// <summary>
    /// <inheritdoc cref="TimeVariables.TargetFixedDelta"/>
    /// </summary>
    public static long TargetFixedDelta
    {
        get => Variables.TargetFixedDelta;
        set => Variables.TargetFixedDelta = value;
    }

    /// <summary>
    /// <inheritdoc cref="TimeVariables.TargetDelta"/>
    /// </summary>
    public static long TargetDelta
    {
        get => Variables.TargetDelta;
        set => Variables.TargetDelta = value;
    }

    /// <summary>
    /// <inheritdoc cref="TimeVariables.Total"/>
    /// </summary>
    public static long Total => Variables.Total;

    /// <summary>
    /// <inheritdoc cref="TimeVariables.Delta"/>
    /// </summary>
    public static long Delta => Variables.Delta;

    internal static ref PreciseVariables Variables => ref Game.Shared.Time.Precise;
}
