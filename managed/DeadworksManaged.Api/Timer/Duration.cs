namespace DeadworksManaged.Api;

/// <summary>
/// Strongly-typed time value that can represent either game ticks or real (wall-clock) time.
/// Created via extension methods: <c>3.Seconds()</c>, <c>64.Ticks()</c>, <c>500.Milliseconds()</c>.
/// </summary>
public readonly record struct Duration
{
    internal long Value { get; }
    internal DurationKind Kind { get; }

    internal Duration(long value, DurationKind kind)
    {
        Value = value;
        Kind = kind;
    }

    internal static Duration FromTicks(long ticks) => new(ticks, DurationKind.Ticks);
    internal static Duration FromMilliseconds(long ms) => new(ms, DurationKind.RealTime);
    internal static Duration FromSeconds(double seconds) => new((long)(seconds * 1000), DurationKind.RealTime);

    public static implicit operator Duration(TimeSpan timeSpan) => FromMilliseconds((long)timeSpan.TotalMilliseconds);
}
