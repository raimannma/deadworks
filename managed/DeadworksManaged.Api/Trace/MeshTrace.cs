using System.Numerics;
using System.Runtime.InteropServices;

namespace DeadworksManaged.Api;

/// <summary>Shape data for a convex mesh trace with bounds and a vertex array.</summary>
[StructLayout(LayoutKind.Sequential, Size = 36)]
public unsafe struct MeshTrace {
	public Vector3 Mins;
	public Vector3 Maxs;
	public Vector3* Vertices;
	public int NumVertices;
}
