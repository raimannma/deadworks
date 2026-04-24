namespace DeadworksManaged.Api;

public unsafe class AbilityResource : NativeEntity {
	private static ReadOnlySpan<byte> Class => "AbilityResource_t"u8;

	internal AbilityResource(nint handle) : base(handle) { }

	private static readonly SchemaAccessor<float> _currentValue = new(Class, "m_flCurrentValue"u8);
	public float CurrentValue { get => _currentValue.Get(Handle); set => *(float*)_currentValue.GetAddress(Handle) = value; }

	private static readonly SchemaAccessor<float> _prevRegenRate = new(Class, "m_flPrevRegenRate"u8);
	public float PrevRegenRate { get => _prevRegenRate.Get(Handle); set => *(float*)_prevRegenRate.GetAddress(Handle) = value; }

	private static readonly SchemaAccessor<float> _maxValue = new(Class, "m_flMaxValue"u8);
	public float MaxValue { get => _maxValue.Get(Handle); set => *(float*)_maxValue.GetAddress(Handle) = value; }

	private static readonly SchemaAccessor<float> _latchTime = new(Class, "m_flLatchTime"u8, 1);
	public float LatchTime { get => _latchTime.Get(Handle); set => _latchTime.Set(Handle, value); }

	private static readonly SchemaAccessor<float> _latchValue = new(Class, "m_flLatchValue"u8, 1);
	public float LatchValue { get => _latchValue.Get(Handle); set => _latchValue.Set(Handle, value); }
}
