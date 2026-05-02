namespace DeadworksManaged.Api;

/// <summary>Observer camera mode. Stored on <see cref="CPlayer_ObserverServices"/> as <c>m_iObserverMode</c>.</summary>
public enum ObserverMode_t : uint {
	None = 0,
	Fixed = 1,
	InEye = 2,
	Chase = 3,
	Roaming = 4,
}
