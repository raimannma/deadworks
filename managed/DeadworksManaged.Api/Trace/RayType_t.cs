namespace DeadworksManaged.Api;

/// <summary>Shape type used for VPhys2 trace queries.</summary>
public enum RayType_t : byte {
	Line = 0,
	Sphere,
	Hull,
	Capsule,
	Mesh,
}
