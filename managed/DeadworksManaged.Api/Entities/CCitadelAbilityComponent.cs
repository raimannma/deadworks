namespace DeadworksManaged.Api;

/// <summary>Ability component on a player pawn. Provides access to stamina/ability resources and the list of equipped abilities.</summary>
public unsafe class CCitadelAbilityComponent : NativeEntity {
	internal CCitadelAbilityComponent(nint handle) : base(handle) { }

	private static readonly SchemaAccessor<byte> _resourceStamina = new("CCitadelAbilityComponent"u8, "m_ResourceStamina"u8);
	public AbilityResource ResourceStamina => new(_resourceStamina.GetAddress(Handle));

	private static readonly SchemaAccessor<byte> _resourceAbility = new("CCitadelAbilityComponent"u8, "m_ResourceAbility"u8);
	public AbilityResource ResourceAbility => new(_resourceAbility.GetAddress(Handle));

	private static readonly SchemaAccessor<byte> _vecAbilities = new("CCitadelAbilityComponent"u8, "m_vecAbilities"u8);

	public IReadOnlyList<CCitadelBaseAbility> Abilities {
		get {
			var result = new List<CCitadelBaseAbility>();
			nint vecAddr = _vecAbilities.GetAddress(Handle);
			int count = NativeInterop.GetUtlVectorSize((void*)vecAddr);
			uint* data = (uint*)NativeInterop.GetUtlVectorData((void*)vecAddr);
			if (data == null || count <= 0) return result;
			for (int i = 0; i < count; i++) {
				void* ent = NativeInterop.GetEntityFromHandle(data[i]);
				if (ent != null)
					result.Add(new CCitadelBaseAbility((nint)ent));
			}
			return result;
		}
	}

	/// <summary>Executes the ability in the given slot. Returns the execution result (0 = success, negative = error).</summary>
	/// <param name="slot">The ability slot to execute.</param>
	/// <param name="altCast">If true, uses alternate cast mode.</param>
	/// <param name="flags">Execution flags passed to the ability system.</param>
	public int ExecuteAbilityBySlot(EAbilitySlot slot, bool altCast = false, byte flags = 0) {
		return NativeInterop.ExecuteAbilityBySlot((void*)Handle, (short)slot, altCast ? (byte)1 : (byte)0, flags);
	}

	/// <summary>Executes an ability by its runtime ability ID. Returns the execution result (0 = success, negative = error).</summary>
	public int ExecuteAbilityByID(int abilityID, bool altCast = false, byte flags = 0) {
		return NativeInterop.ExecuteAbilityByID((void*)Handle, abilityID, altCast ? (byte)1 : (byte)0, flags);
	}

	/// <summary>Executes a specific ability entity. Returns the execution result (0 = success, negative = error).</summary>
	public int ExecuteAbility(CBaseEntity ability, bool altCast = false, byte flags = 0) {
		return NativeInterop.ExecuteAbility((void*)Handle, (void*)ability.Handle, altCast ? (byte)1 : (byte)0, flags);
	}

	/// <summary>Gets the ability entity in the given slot, or null if no ability occupies that slot.</summary>
	public CCitadelBaseAbility? GetAbilityBySlot(EAbilitySlot slot) {
		void* result = NativeInterop.GetAbilityBySlot((void*)Handle, (short)slot);
		return result != null ? new CCitadelBaseAbility((nint)result) : null;
	}

	/// <summary>Activates or deactivates an ability (toggle). This is the actual activation path for most abilities.</summary>
	public void ToggleActivate(CBaseEntity ability, bool activate = true) {
		NativeInterop.ToggleActivate((void*)ability.Handle, activate ? (byte)1 : (byte)0);
	}
}
