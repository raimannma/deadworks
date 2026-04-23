namespace DeadworksManaged.Api;

/// <summary>Jump ability entity tracking air jump/wall jump counters for a hero.</summary>
[NativeClass("CCitadel_Ability_Jump")]
public sealed unsafe class CCitadel_Ability_Jump : CBaseEntity {
	internal CCitadel_Ability_Jump(nint handle) : base(handle) { }
	private static ReadOnlySpan<byte> Class => "CCitadel_Ability_Jump"u8;

	private static readonly SchemaAccessor<int> _desiredAirJumpCount = new(Class, "m_nDesiredAirJumpCount"u8);
	public int DesiredAirJumpCount { get => _desiredAirJumpCount.Get(Handle); set => _desiredAirJumpCount.Set(Handle, value); }

	private static readonly SchemaAccessor<int> _executedAirJumpCount = new(Class, "m_nExecutedAirJumpCount"u8);
	public int ExecutedAirJumpCount { get => _executedAirJumpCount.Get(Handle); set => _executedAirJumpCount.Set(Handle, value); }

	private static readonly SchemaAccessor<sbyte> _consecutiveAirJumps = new(Class, "m_nConsecutiveAirJumps"u8);
	public sbyte ConsecutiveAirJumps { get => _consecutiveAirJumps.Get(Handle); set => _consecutiveAirJumps.Set(Handle, value); }

	private static readonly SchemaAccessor<sbyte> _consecutiveWallJumps = new(Class, "m_nConsecutiveWallJumps"u8);
	public sbyte ConsecutiveWallJumps { get => _consecutiveWallJumps.Get(Handle); set => _consecutiveWallJumps.Set(Handle, value); }
}
