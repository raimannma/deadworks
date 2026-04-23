using System.Runtime.InteropServices;

namespace DeadworksManaged.Api;

/// <summary>Collision attributes of a physics body: interaction layer masks, entity/owner IDs, and collision group.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct RnCollisionAttr_t {
	public ulong InteractsAs;
	public ulong InteractsWith;
	public ulong InteractsExclude;
	public uint EntityId;
	public uint OwnerId;
	public ushort HierarchyId;
	public CollisionGroup CollisionGroup;
	public CollisionFunctionMask_t CollisionFunctionMask;
}
