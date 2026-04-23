using System.Numerics;
using System.Runtime.InteropServices;

namespace DeadworksManaged.Api;

/// <summary>Shape data for a line (ray) trace with an optional radius for a swept sphere.</summary>
[StructLayout(LayoutKind.Sequential, Size = 16)]
public struct LineTrace {
	public Vector3 StartOffset;
	public float Radius;
}
