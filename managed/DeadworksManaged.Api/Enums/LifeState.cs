namespace DeadworksManaged.Api;

/// <summary>Entity life-cycle state (alive, dying, dead, respawning).</summary>
public enum LifeState : uint {
	Alive = 0x0,
	Dying = 0x1,
	Dead = 0x2,
	Respawnable = 0x3,
	Respawning = 0x4,
}
