namespace DeadworksManaged.Api;

/// <summary>String comparison mode used when matching entity designer names in trace results.</summary>
public enum NameMatchType {
	Exact = 0,
	StartsWith = 1,
	EndsWith = 2,
	Contains = 3,
}
