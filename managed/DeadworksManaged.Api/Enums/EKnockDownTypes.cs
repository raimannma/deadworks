namespace DeadworksManaged.Api;

/// <summary>Knockdown animation type applied to a hero.</summary>
public enum EKnockDownTypes : uint {
	KnockdownLarge = 0x0,
	KnockdownMedium = 0x1,
	KnockdownSmall = 0x2,
	KnockdownPancake = 0x3,
	KnockdownParried = 0x4,
	ENumKnockdowns = 0x5,
	EKnockdownInvalid = 0x5,
}
