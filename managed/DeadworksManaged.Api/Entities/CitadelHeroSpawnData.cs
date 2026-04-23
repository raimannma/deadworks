namespace DeadworksManaged.Api;

/// <summary>Wraps CitadelHeroSpawnData_t - per-spawn hero state including the resolved hero ID.</summary>
public unsafe class CitadelHeroSpawnData : NativeEntity {
	private static ReadOnlySpan<byte> Class => "CitadelHeroSpawnData_t"u8;

	internal CitadelHeroSpawnData(nint handle) : base(handle) { }

	private static readonly SchemaAccessor<uint> _nHeroID = new(Class, "m_nHeroID"u8);
	public uint HeroID { get => _nHeroID.Get(Handle); set => _nHeroID.Set(Handle, value); }
}
