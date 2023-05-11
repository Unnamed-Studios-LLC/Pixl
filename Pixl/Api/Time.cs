namespace Pixl;

public static class Time
{
    /// <summary>
    /// <inheritdoc cref="Api.FixedTotal"/>
    /// </summary>
    public static float FixedTotal => Game.Current.TimeTotal;

    /// <summary>
    /// <inheritdoc cref="Api.FixedUpdateDelta"/>
    /// </summary>
    public static float FixedUpdateDelta => Game.Current.FixedUpdateDelta;

    /// <summary>
    /// <inheritdoc cref="Api.Precise"/>
    /// </summary>
    public static PreciseTime Precise => Game.Current.Precise;

    /// <summary>
    /// <inheritdoc cref="PreciseTime.TicksPerSecond"/>
    /// </summary>
    public static long PreciseTicksPerSecond => PreciseTime.TicksPerSecond;

    /// <summary>
    /// <inheritdoc cref="Api.TargetUpdateDelta"/>
    /// </summary>
    public static float TargetUpdateDelta => Game.Current.TargetUpdateDelta;

    /// <summary>
    /// <inheritdoc cref="Api.Total"/>
    /// </summary>
    public static float Total => Game.Current.TimeTotal;

    /// <summary>
    /// <inheritdoc cref="Api.UpdateDelta"/>
    /// </summary>
    public static float UpdateDelta => Game.Current.UpdateDelta;
}
