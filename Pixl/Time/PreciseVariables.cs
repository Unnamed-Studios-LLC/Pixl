using System.Diagnostics;

namespace Pixl;

public struct PreciseVariables
{
    /// <summary>
    /// The amount of time ticks per second. All precise variables can be divided by this number for a conversion to seconds.
    /// </summary>
    public static long TicksPerSecond => Stopwatch.Frequency;

    /// <summary>
    /// <inheritdoc cref="TimeVariables.FixedDelta"/>
    /// </summary>
    public long FixedDelta { get; internal set; }

    /// <summary>
    /// <inheritdoc cref="TimeVariables.FixedTimeTotal"/>
    /// </summary>
    public long FixedTotal { get; internal set; }

    /// <summary>
    /// <inheritdoc cref="TimeVariables.TargetUpdateDelta"/>
    /// </summary>
    public long TargetFixedDelta { get; internal set; }

    /// <summary>
    /// <inheritdoc cref="TimeVariables.TargetFixedDelta"/>
    /// </summary>
    public long TargetDelta { get; internal set; }

    /// <summary>
    /// <inheritdoc cref="TimeVariables.Total"/>
    /// </summary>
    public long Total { get; internal set; }

    /// <summary>
    /// <inheritdoc cref="TimeVariables.Delta"/>
    /// </summary>
    public long Delta { get; internal set; }
}