using System.Runtime.InteropServices;

namespace DeadworksManaged.Api;

internal static class CTraceFilterVTable {
	public static readonly nint Simple;
	public static readonly nint WithEntityFilter;

	[UnmanagedCallersOnly]
	private static unsafe void Destructor(CTraceFilter* filter, byte unk) { }

	[UnmanagedCallersOnly]
	private static unsafe byte ShouldHitEntitySimple() => 1;

	[UnmanagedCallersOnly]
	private static unsafe byte ShouldHitEntity(CTraceFilter* filter, nint entity) {
		if (entity == 0) return 0;
		var ent = new CBaseEntity(entity);
		var idx = (uint)ent.EntityIndex;
		return filter->QueryShapeAttributes.EntityIdsToIgnore[0] != idx &&
		       filter->QueryShapeAttributes.EntityIdsToIgnore[1] != idx ? (byte)1 : (byte)0;
	}

	static unsafe CTraceFilterVTable() {
		Simple = Marshal.AllocHGlobal(sizeof(nint) * 2);
		var vtable = new Span<nint>((void*)Simple, 2);
		vtable[0] = (nint)(delegate* unmanaged<CTraceFilter*, byte, void>)&Destructor;
		vtable[1] = (nint)(delegate* unmanaged<byte>)&ShouldHitEntitySimple;

		WithEntityFilter = Marshal.AllocHGlobal(sizeof(nint) * 2);
		var funcTable = new Span<nint>((void*)WithEntityFilter, 2);
		funcTable[0] = (nint)(delegate* unmanaged<CTraceFilter*, byte, void>)&Destructor;
		funcTable[1] = (nint)(delegate* unmanaged<CTraceFilter*, nint, byte>)&ShouldHitEntity;
	}
}
