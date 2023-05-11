using System.Diagnostics;

namespace Pixl;

public class PreciseTime
{
    /// <summary>
    /// The amount of time ticks per second. All precise variables can be divided by this number for a conversion to seconds.
    /// </summary>
    public static long TicksPerSecond => Stopwatch.Frequency;

    /// <summary>
    /// <inheritdoc cref="Api.FixedTimeTotal"/>
    /// </summary>
    public long FixedTotal { get; internal set; } = 0;

    /// <summary>
    /// <inheritdoc cref="Api.FixedUpdateDelta"/>
    /// </summary>
    public long FixedUpdateDelta { get; internal set; } = TicksPerSecond / 30;

    /// <summary>
    /// <inheritdoc cref="Api.TargetUpdateDelta"/>
    /// </summary>
    public long TargetUpdateDelta { get; internal set; } = TicksPerSecond / 60;

    /// <summary>
    /// <inheritdoc cref="Api.Total"/>
    /// </summary>
    public long Total { get; internal set; } = 0;

    /// <summary>
    /// <inheritdoc cref="Api.UpdateDelta"/>
    /// </summary>
    public long UpdateDelta { get; internal set; } = 0;
}
