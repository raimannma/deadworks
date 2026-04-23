namespace DeadworksManaged.Api;

/// <summary>Base class for all Deadlock abilities (hero abilities, items, innates, etc.).</summary>
[NativeClass("CCitadelBaseAbility")]
public unsafe class CCitadelBaseAbility : CBaseEntity {
	internal CCitadelBaseAbility(nint handle) : base(handle) { }
	private static ReadOnlySpan<byte> Class => "CCitadelBaseAbility"u8;

	private static readonly SchemaAccessor<short> _abilitySlot = new(Class, "m_eAbilitySlot"u8);
	private static readonly SchemaAccessor<bool> _channeling = new(Class, "m_bChanneling"u8);
	private static readonly SchemaAccessor<bool> _canBeUpgraded = new(Class, "m_bCanBeUpgraded"u8);
	private static readonly SchemaAccessor<bool> _toggleState = new(Class, "m_bToggleState"u8);
	private static readonly SchemaAccessor<float> _cooldownEnd = new(Class, "m_flCooldownEnd"u8);
	private static readonly SchemaAccessor<float> _cooldownStart = new(Class, "m_flCooldownStart"u8);

	private static int UpgradeBitsOffset => _abilitySlot.Offset - 0x20;

	public int UpgradeBits {
		get => *(short*)((byte*)Handle + UpgradeBitsOffset + 2);
		set => NativeInterop.SetUpgradeBits((void*)Handle, value);
	}
	public EAbilitySlot AbilitySlot => (EAbilitySlot)_abilitySlot.Get(Handle);
	public bool IsChanneling => _channeling.Get(Handle);
	public bool CanBeUpgraded { get => _canBeUpgraded.Get(Handle); set => _canBeUpgraded.Set(Handle, value); }
	public bool ToggleState => _toggleState.Get(Handle);
	public float CooldownEnd { get => _cooldownEnd.Get(Handle); set => _cooldownEnd.Set(Handle, value); }
	public float CooldownStart { get => _cooldownStart.Get(Handle); set => _cooldownStart.Set(Handle, value); }
	public bool IsUnlocked => (UpgradeBits & 1) != 0;

	public bool IsSignature => AbilitySlot >= EAbilitySlot.Signature1 && AbilitySlot <= EAbilitySlot.Signature4;
	public bool IsActiveItem => AbilitySlot >= EAbilitySlot.ActiveItem1 && AbilitySlot <= EAbilitySlot.ActiveItem4;
	public bool IsInnate => AbilitySlot >= EAbilitySlot.Innate1 && AbilitySlot <= EAbilitySlot.Innate3;
	public bool IsWeapon => AbilitySlot >= EAbilitySlot.WeaponSecondary && AbilitySlot <= EAbilitySlot.WeaponMelee;
	public bool IsItem => (SubclassVData?.Name ?? "").StartsWith("upgrade_");
	public string AbilityName => SubclassVData?.Name ?? "";
}
