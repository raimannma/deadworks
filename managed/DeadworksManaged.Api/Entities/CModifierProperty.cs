namespace DeadworksManaged.Api;

/// <summary>
/// Wraps CModifierProperty - manages modifier state bits on an entity.
/// Uses CNetworkVarChainer to chain network notifications to the owning entity.
/// </summary>
public unsafe class CModifierProperty : NativeEntity {
	internal CModifierProperty(nint handle) : base(handle) { }
	private static ReadOnlySpan<byte> Class => "CModifierProperty"u8;

	private static readonly SchemaAccessor<uint> _hOwner = new(Class, "m_hOwner"u8);

	/// <summary>The entity that owns this modifier property.</summary>
	public CBaseEntity? Owner => CBaseEntity.FromHandle(_hOwner.Get(Handle));

	private static readonly SchemaAccessor<byte> _vecModifiers = new(Class, "m_vecModifiers"u8);

	/// <summary>Returns all active modifiers on this entity.</summary>
	public IReadOnlyList<CBaseModifier> Modifiers {
		get {
			var result = new List<CBaseModifier>();
			nint vecAddr = _vecModifiers.GetAddress(Handle);
			int count = NativeInterop.GetUtlVectorSize((void*)vecAddr);
			nint* data = (nint*)NativeInterop.GetUtlVectorData((void*)vecAddr);
			if (data == null || count <= 0) return result;
			for (int i = 0; i < count; i++) {
				if (data[i] != 0)
					result.Add(new CBaseModifier(data[i]));
			}
			return result;
		}
	}

	/// <summary>Returns true if this entity has an active modifier with the specified subclass VData name.</summary>
	public bool HasModifier(string name) {
		nint vecAddr = _vecModifiers.GetAddress(Handle);
		int count = NativeInterop.GetUtlVectorSize((void*)vecAddr);
		nint* data = (nint*)NativeInterop.GetUtlVectorData((void*)vecAddr);
		if (data == null || count <= 0) return false;
		for (int i = 0; i < count; i++) {
			if (data[i] == 0) continue;
			// m_pSubclassVData datamap field at offset 0x10
			nint pVData = *(nint*)((byte*)data[i] + 0x10);
			if (pVData == 0) continue;
			// Name string pointer at VData + 0x10
			nint namePtr = *(nint*)((byte*)pVData + 0x10);
			if (namePtr != 0 && System.Runtime.InteropServices.Marshal.PtrToStringUTF8(namePtr) == name)
				return true;
		}
		return false;
	}

	private static readonly SchemaArrayAccessor<uint> _enabledStateMask = new(Class, "m_bvEnabledStateMask"u8);

	/// <summary>Sets or clears the specified modifier state bit on this entity, notifying the network if changed.</summary>
	public void SetModifierState(EModifierState state, bool enabled) {
		int s = (int)state;
		int index = s >> 5;
		uint bit = 1u << (s & 0x1F);
		uint current = _enabledStateMask.Get(Handle, index);
		uint updated = enabled ? (current | bit) : (current & ~bit);
		if (current != updated)
			_enabledStateMask.Set(Handle, index, updated);
	}

	/// <summary>Returns true if the specified modifier state bit is currently set on this entity.</summary>
	public bool HasModifierState(EModifierState state) {
		int s = (int)state;
		int index = s >> 5;
		uint bit = 1u << (s & 0x1F);
		return (_enabledStateMask.Get(Handle, index) & bit) != 0;
	}
}
