using System.Numerics;
using System.Runtime.InteropServices;

namespace DeadworksManaged.Api;

/// <summary>Shape data for a sphere trace at a fixed center point.</summary>
[StructLayout(LayoutKind.Sequential, Size = 16)]
public struct SphereTrace {
	public Vector3 Center;
	public float Radius;
}
