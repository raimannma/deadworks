namespace DeadworksManaged.Api;

/// <summary>
/// Observer/spectator state component on a <see cref="CBasePlayerPawn"/>.
/// </summary>
public unsafe class CPlayer_ObserverServices : NativeEntity {
	internal CPlayer_ObserverServices(nint handle) : base(handle) { }

	private static readonly SchemaAccessor<byte> _iObserverMode = new("CPlayer_ObserverServices"u8, "m_iObserverMode"u8);
	private static readonly SchemaAccessor<uint> _hObserverTarget = new("CPlayer_ObserverServices"u8, "m_hObserverTarget"u8);
	private static readonly SchemaAccessor<int> _iObserverLastMode = new("CPlayer_ObserverServices"u8, "m_iObserverLastMode"u8);

	/// <summary>The current networked observer mode.</summary>
	public ObserverMode_t ObserverMode => (ObserverMode_t)_iObserverMode.Get(Handle);

	/// <summary>The previous non-zero observer mode, restored when re-entering observer state.</summary>
	public ObserverMode_t ObserverLastMode {
		get => (ObserverMode_t)_iObserverLastMode.Get(Handle);
		set => _iObserverLastMode.Set(Handle, (int)value);
	}

	/// <summary>The currently observed entity, or null if no target.</summary>
	public CBaseEntity? ObserverTarget {
		get {
			uint h = _hObserverTarget.Get(Handle);
			return h == CBaseEntity.InvalidEntityHandle ? null : CBaseEntity.FromHandle(h);
		}
	}

	/// <summary>
	/// Validates a candidate observer target. Checks that the target is alive and connected,
	/// isn't engine-frozen, and isn't a spectator.
	/// </summary>
	public bool IsValidObserverTarget(CBaseEntity? target) {
		if (target is null || !target.IsValid || !target.IsAlive) return false;
		if ((target.Flags & EntityFlags.Frozen) != 0) return false;
		if (target.TeamNum == 3) return false;
		return true;
	}

	/// <summary>
	/// Sets the observer mode.
	/// </summary>
	public void SetObserverMode(ObserverMode_t mode) {
		NativeInterop.ObserverServicesSetMode((void*)Handle, (int)mode);
	}

	/// <summary>
	/// Sets the observed entity. Pass null to clear.
	/// </summary>
	public bool SetObserverTarget(CBaseEntity? target) {
		nint targetHandle = target?.Handle ?? 0;
		return NativeInterop.ObserverServicesSetTarget((void*)Handle, (void*)targetHandle) != 0;
	}
}
