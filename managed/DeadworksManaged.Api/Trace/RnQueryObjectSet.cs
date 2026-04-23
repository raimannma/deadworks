namespace DeadworksManaged.Api;

/// <summary>Bitmask controlling which object sets (static, dynamic, locatable) are included in a trace query.</summary>
[Flags]
public enum RnQueryObjectSet : byte {
	Static = 1 << 0,
	Keyframed = 1 << 1,
	Dynamic = 1 << 2,
	Locatable = 1 << 3,
	AllGameEntities = Keyframed | Dynamic | Locatable,
	All = Static | AllGameEntities,
}
