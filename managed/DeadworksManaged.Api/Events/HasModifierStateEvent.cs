namespace DeadworksManaged.Api;

/// <summary>
/// Event data for the HasModifierState hook.
/// Set <see cref="Result"/> to override the return value for a specific modifier state,
/// or leave it null to fall through to the original game logic.
/// </summary>
public sealed class HasModifierStateEvent {
	/// <summary>The entity being queried.</summary>
	public required CBaseEntity Entity { get; init; }

	/// <summary>The modifier state being checked.</summary>
	public required EModifierState State { get; init; }

	/// <summary>
	/// Set to <c>true</c> to force the state as present, <c>false</c> to force it as absent,
	/// or leave <c>null</c> to fall through to the original game logic.
	/// </summary>
	public bool? Result { get; set; }
}
