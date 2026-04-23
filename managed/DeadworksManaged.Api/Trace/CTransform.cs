using System.Numerics;
using System.Runtime.InteropServices;

namespace DeadworksManaged.Api;

/// <summary>Position + orientation transform (translation and quaternion rotation).</summary>
[StructLayout(LayoutKind.Explicit)]
public struct CTransform {
	[FieldOffset(0x00)] public Vector3 Position;
	[FieldOffset(0x0C)] private int _pad;
	[FieldOffset(0x10)] public Quaternion Orientation;
}
