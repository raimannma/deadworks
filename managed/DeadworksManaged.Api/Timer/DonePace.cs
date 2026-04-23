namespace DeadworksManaged.Api;

/// <summary>The sequence is finished.</summary>
internal sealed class DonePace : Pace
{
    internal static readonly DonePace Instance = new();
    internal DonePace() { }
}
