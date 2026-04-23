namespace DeadworksManaged.Api;

/// <summary>Dash ability entity tracking consecutive air/down dash counters for a hero.</summary>
[NativeClass("CCitadel_Ability_Dash")]
public sealed unsafe class CCitadel_Ability_Dash : CBaseEntity {
	internal CCitadel_Ability_Dash(nint handle) : base(handle) { }
	private static ReadOnlySpan<byte> Class => "CCitadel_Ability_Dash"u8;

	private static readonly SchemaAccessor<sbyte> _consecutiveAirDashes = new(Class, "m_nConsecutiveAirDashes"u8);
	public sbyte ConsecutiveAirDashes { get => _consecutiveAirDashes.Get(Handle); set => _consecutiveAirDashes.Set(Handle, value); }

	private static readonly SchemaAccessor<sbyte> _consecutiveDownDashes = new(Class, "m_nConsecutiveDownDashes"u8);
	public sbyte ConsecutiveDownDashes { get => _consecutiveDownDashes.Get(Handle); set => _consecutiveDownDashes.Set(Handle, value); }
}
