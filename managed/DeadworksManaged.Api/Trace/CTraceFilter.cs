using System.Runtime.InteropServices;

namespace DeadworksManaged.Api;

/// <summary>
/// Trace filter passed to VPhys2 TraceShape. Embeds a vtable pointer (managed vtable with a destructor and ShouldHitEntity callback)
/// and a <see cref="RnQueryShapeAttr_t"/>. Construct with <c>new CTraceFilter()</c> for entity-aware filtering.
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 8, Size = 72)]
public struct CTraceFilter {
	[FieldOffset(0x0)] private nint _vtable;
	[FieldOffset(0x8)] public RnQueryShapeAttr_t QueryShapeAttributes;
	[FieldOffset(0x40)] public bool IterateEntities;

	public CTraceFilter() {
		_vtable = CTraceFilterVTable.WithEntityFilter;
		QueryShapeAttributes = new RnQueryShapeAttr_t();
	}

	public CTraceFilter(bool checkIgnoredEntities) {
		_vtable = checkIgnoredEntities ? CTraceFilterVTable.WithEntityFilter : CTraceFilterVTable.Simple;
		QueryShapeAttributes = new RnQueryShapeAttr_t();
	}

	internal void EnsureValid() {
		if (_vtable == 0)
			_vtable = CTraceFilterVTable.WithEntityFilter;
	}
}
