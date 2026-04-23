namespace DeadworksManaged.Api;

/// <summary>Hero component on a player pawn. Exposes the inline spawn data for the current hero.</summary>
public unsafe class CCitadelHeroComponent : NativeEntity {
	private static ReadOnlySpan<byte> Class => "CCitadelHeroComponent"u8;

	internal CCitadelHeroComponent(nint handle) : base(handle) { }

	private static readonly SchemaAccessor<byte> _spawnedHero = new(Class, "m_spawnedHero"u8);
	public CitadelHeroSpawnData SpawnedHero => new(_spawnedHero.GetAddress(Handle));
}
