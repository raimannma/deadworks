using System.Numerics;
using System.Runtime.InteropServices;

namespace DeadworksManaged.Api;

/// <summary>Shape data for a capsule trace between two center points with a radius.</summary>
[StructLayout(LayoutKind.Sequential, Size = 28)]
public struct CapsuleTrace {
	public Vector3 Center0;
	public Vector3 Center1;
	public float Radius;
}
