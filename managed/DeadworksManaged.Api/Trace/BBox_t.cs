using System.Numerics;
using System.Runtime.InteropServices;

namespace DeadworksManaged.Api;

/// <summary>Axis-aligned bounding box with min/max corners.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct BBox_t {
	public Vector3 Mins;
	public Vector3 Maxs;
}
