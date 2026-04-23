namespace DeadworksManaged.Api;

/// <summary>Base player pawn entity. Provides access to the owning controller.</summary>
[NativeClass("CBasePlayerPawn")]
public unsafe class CBasePlayerPawn : CBaseEntity {
	internal CBasePlayerPawn(nint handle) : base(handle) { }

	private static readonly SchemaAccessor<uint> _hController = new("CBasePlayerPawn"u8, "m_hController"u8);
	public CBasePlayerController? Controller {
		get {
			uint handle = _hController.Get(Handle);
			if (handle == 0xFFFFFFFF) return null;
			void* ptr = NativeInterop.GetEntityFromHandle(handle);
			return ptr != null ? new CBasePlayerController((nint)ptr) : null;
		}
	}
}
