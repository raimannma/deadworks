using System.Numerics;

namespace DeadworksManaged.Api;

/// <summary>Deadlock hero pawn. The in-game physical representation of a player. Provides currency, abilities, movement state, stamina, and eye angles.</summary>
[NativeClass("CCitadelPlayerPawn")]
public sealed unsafe class CCitadelPlayerPawn : CBasePlayerPawn {
	internal CCitadelPlayerPawn(nint handle) : base(handle) { }

	private static readonly SchemaAccessor<uint> _hCitadelController = new("CBasePlayerPawn"u8, "m_hController"u8);
	public new CCitadelPlayerController? Controller {
		get {
			uint handle = _hCitadelController.Get(Handle);
			if (handle == 0xFFFFFFFF) return null;
			void* ptr = NativeInterop.GetEntityFromHandle(handle);
			return ptr != null ? new CCitadelPlayerController((nint)ptr) : null;
		}
	}

	public PlayerDataGlobal? PlayerData => Controller?.PlayerDataGlobal;

	private static readonly SchemaAccessor<byte> _abilityComp = new("CCitadelPlayerPawn"u8, "m_CCitadelAbilityComponent"u8);
	public CCitadelAbilityComponent AbilityComponent => new(_abilityComp.GetAddress(Handle));

	private static readonly SchemaAccessor<byte> _heroComp = new("CCitadelPlayerPawn"u8, "m_CCitadelHeroComponent"u8);
	public CCitadelHeroComponent HeroComponent => new(_heroComp.GetAddress(Handle));

	public Heroes HeroID => (Heroes)HeroComponent.SpawnedHero.HeroID;

	private static readonly SchemaAccessor<bool> _inRegenZone = new("CCitadelPlayerPawn"u8, "m_bInRegenerationZone"u8);
	public bool InRegenerationZone => _inRegenZone.Get(Handle);

	private static readonly SchemaAccessor<Vector3> _eyeAngles = new("CCitadelPlayerPawn"u8, "m_angEyeAngles"u8);
	/// <summary>Networked eye angles (quantized to 11 bits, ~0.18 precision). Use ViewAngles for raw precision.</summary>
	public Vector3 EyeAngles => _eyeAngles.Get(Handle);

	private static readonly SchemaAccessor<byte> _viewOffset = new("CBaseModelEntity"u8, "m_vecViewOffset"u8);
	/// <summary>Eye position (AbsOrigin + ViewOffset). This is where the camera sits.</summary>
	public unsafe Vector3 EyePosition {
		get {
			var pos = Position;
			nint voBase = _viewOffset.GetAddress(Handle);
			// CNetworkViewOffsetVector: m_vecX at +0x10, m_vecY at +0x18, m_vecZ at +0x20
			// Each is a CNetworkedQuantizedFloat with float Value at +0x00
			float voX = *(float*)(voBase + 0x10);
			float voY = *(float*)(voBase + 0x18);
			float voZ = *(float*)(voBase + 0x20);
			return new Vector3(pos.X + voX, pos.Y + voY, pos.Z + voZ);
		}
	}

	private static readonly SchemaAccessor<Vector3> _clientCamera = new("CCitadelPlayerPawn"u8, "m_angClientCamera"u8);
	/// <summary>Client camera angles for SourceTV/spectating.</summary>
	public Vector3 CameraAngles => _clientCamera.Get(Handle);

	/// <summary>Raw server-side view angles from CUserCmd (v_angle). Full float precision, no quantization.</summary>
	public unsafe Vector3 ViewAngles {
		get {
			// v_angle is at offset 0xC48 in CBasePlayerPawn (not networked, server-only)
			float* p = (float*)(Handle + 0xC48);
			return new Vector3(p[0], p[1], p[2]);
		}
	}

	private static readonly SchemaAccessor<int> _level = new("CCitadelPlayerPawn"u8, "m_nLevel"u8);
	public int Level { get => _level.Get(Handle); set => _level.Set(Handle, value); }

	private static readonly SchemaArrayAccessor<int> _currencies = new("CCitadelPlayerPawn"u8, "m_nCurrencies"u8);
	public int GetCurrency(ECurrencyType type) => _currencies.Get(Handle, (int)type);
	public void SetCurrency(ECurrencyType type, int value) => _currencies.Set(Handle, (int)type, value);

	/// <summary>Adds or removes currency from this pawn (e.g. gold, ability points). Use negative <paramref name="amount"/> to spend.</summary>
	public void ModifyCurrency(ECurrencyType type, int amount, ECurrencySource source,
								bool silent = false, bool forceGain = false, bool spendOnly = false) {
		NativeInterop.ModifyCurrency((void*)Handle, (uint)type, amount, (uint)source,
									  silent ? (byte)1 : (byte)0, spendOnly ? (byte)1 : (byte)0, forceGain ? (byte)1 : (byte)0,
									  (void*)0, (void*)0);
	}

	/// <summary>
	/// Full pawn-level hero reset: clears loadout, removes items, re-adds starting abilities from VData, resets level.
	/// </summary>
	public void ResetHero(bool resetAbilities = true) {
		if (NativeInterop.ResetHero != null)
			NativeInterop.ResetHero((void*)Handle, resetAbilities ? (byte)1 : (byte)0);
	}

	/// <summary>Removes an ability from this pawn by internal ability name. Returns true on success.</summary>
	public bool RemoveAbility(string abilityName) {
		Span<byte> utf8 = Utf8.Encode(abilityName, stackalloc byte[Utf8.Size(abilityName)]);
		fixed (byte* ptr = utf8) {
			return NativeInterop.RemoveAbility((void*)Handle, ptr) != 0;
		}
	}

	/// <summary>Adds an ability to this pawn by internal ability name into the given slot. Returns the new ability entity, or null on failure.</summary>
	public CBaseEntity? AddAbility(string abilityName, ushort slot) {
		Span<byte> utf8 = Utf8.Encode(abilityName, stackalloc byte[Utf8.Size(abilityName)]);
		fixed (byte* ptr = utf8) {
			void* result = NativeInterop.AddAbility((void*)Handle, ptr, slot);
			return result != null ? new CBaseEntity((nint)result) : null;
		}
	}

	/// <summary>
	/// Gives an item to this pawn by internal item name (e.g. "upgrade_sprint_booster").
	/// <param name="itemName">Internal item name.</param>
	/// <param name="enhanced">Should the enhanced version of the item be given</param>
	/// Returns the new item entity, or null on failure.
	/// </summary>
	public CBaseEntity? AddItem(string itemName, bool enhanced = false) {
		Span<byte> utf8 = Utf8.Encode(itemName, stackalloc byte[Utf8.Size(itemName)]);
		fixed (byte* ptr = utf8) {
			var bits = UpgradeFlags.Owned;
			if (enhanced) bits |= UpgradeFlags.Enhanced;
			void* result = NativeInterop.AddItem((void*)Handle, ptr, (int)bits);
			return result != null ? new CBaseEntity((nint)result) : null;
		}
	}


	/// <summary>
	/// Removes an item from this pawn by name, using the ability removal path.
	/// This bypasses sell checks and does not refund gold - it directly removes the item entity
	/// from the ability component, slot table, and network state.
	/// Returns true on success.
	/// </summary>
	public bool RemoveItem(string itemName) {
		return RemoveAbility(itemName);
	}

	/// <summary>Executes the ability in the given slot on this pawn's ability component.</summary>
	public int ExecuteAbilityBySlot(EAbilitySlot slot, bool altCast = false, byte flags = 0) {
		return AbilityComponent.ExecuteAbilityBySlot(slot, altCast, flags);
	}

	/// <summary>Executes an ability by its runtime ability ID on this pawn's ability component.</summary>
	public int ExecuteAbilityByID(int abilityID, bool altCast = false, byte flags = 0) {
		return AbilityComponent.ExecuteAbilityByID(abilityID, altCast, flags);
	}

	/// <summary>Executes a specific ability entity on this pawn's ability component.</summary>
	public int ExecuteAbility(CBaseEntity ability, bool altCast = false, byte flags = 0) {
		return AbilityComponent.ExecuteAbility(ability, altCast, flags);
	}

	/// <summary>Gets the ability entity in the given slot from this pawn's ability component.</summary>
	public CBaseEntity? GetAbilityBySlot(EAbilitySlot slot) {
		return AbilityComponent.GetAbilityBySlot(slot);
	}

	/// <summary>Activates or deactivates an ability on this pawn (toggle). This is the actual activation path for most abilities.</summary>
	public void ToggleActivate(CBaseEntity ability, bool activate = true) {
		AbilityComponent.ToggleActivate(ability, activate);
	}

	/// <summary>
	/// Sells an item from this pawn by internal item name.
	/// This always refunds gold (at normal or full sell price) and will fail for items that cannot be sold.
	/// <param name="itemName">Internal item name (e.g. "upgrade_sprint_booster").</param>
	/// <param name="fullRefund">If true, skips partial sell-back tracking (item treated as fully refunded).</param>
	/// <param name="forceSellPrice">If true, forces the item to sell at full sell price even if conditions aren't met.</param>
	/// Returns true on success, false if the item was not found or cannot be sold.
	/// </summary>
	public bool SellItem(string itemName, bool fullRefund = false, bool forceSellPrice = false) {
		Span<byte> utf8 = Utf8.Encode(itemName, stackalloc byte[Utf8.Size(itemName)]);
		fixed (byte* ptr = utf8) {
			return NativeInterop.SellItem((void*)Handle, ptr, fullRefund ? (byte)1 : (byte)0, forceSellPrice ? (byte)1 : (byte)0) != 0;
		}
	}
}
