using System.Numerics;
using System.Runtime.InteropServices;

namespace DeadworksManaged.Api;

/// <summary>Shape data for an AABB hull trace swept along a ray.</summary>
[StructLayout(LayoutKind.Sequential, Size = 24)]
public struct HullTrace {
	public Vector3 Mins;
	public Vector3 Maxs;
}
