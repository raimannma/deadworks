namespace DeadworksManaged.Api;

public static class DurationExtensions
{
    /// <summary>Creates a tick-based duration. Usage: <c>64.Ticks()</c></summary>
    public static Duration Ticks(this int ticks) => Duration.FromTicks(ticks);

    /// <summary>Creates a tick-based duration. Usage: <c>64L.Ticks()</c></summary>
    public static Duration Ticks(this long ticks) => Duration.FromTicks(ticks);

    /// <summary>Creates a real-time duration in seconds. Usage: <c>3.Seconds()</c></summary>
    public static Duration Seconds(this int seconds) => Duration.FromSeconds(seconds);

    /// <summary>Creates a real-time duration in seconds. Usage: <c>1.5.Seconds()</c></summary>
    public static Duration Seconds(this double seconds) => Duration.FromSeconds(seconds);

    /// <summary>Creates a real-time duration in milliseconds. Usage: <c>500.Milliseconds()</c></summary>
    public static Duration Milliseconds(this int ms) => Duration.FromMilliseconds(ms);

    /// <summary>Creates a real-time duration in milliseconds. Usage: <c>500L.Milliseconds()</c></summary>
    public static Duration Milliseconds(this long ms) => Duration.FromMilliseconds(ms);
}
