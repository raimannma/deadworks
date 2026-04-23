namespace DeadworksManaged.Api;

/// <summary>Execute again after the specified duration.</summary>
internal sealed class WaitPace : Pace
{
    public Duration Delay { get; }
    internal WaitPace(Duration delay) => Delay = delay;
}
