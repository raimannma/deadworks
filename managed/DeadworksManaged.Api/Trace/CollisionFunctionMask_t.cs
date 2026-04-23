namespace DeadworksManaged.Api;

/// <summary>Flags controlling which collision callbacks are enabled on a physics object.</summary>
[Flags]
public enum CollisionFunctionMask_t : byte {
	EnableSolidContact = 1 << 0,
	EnableTraceQuery = 1 << 1,
	EnableTouchEvent = 1 << 2,
	EnableSelfCollisions = 1 << 3,
	IgnoreForHitboxTest = 1 << 4,
	EnableTouchPersists = 1 << 5,
}
