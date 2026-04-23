using System.Runtime.InteropServices;

namespace DeadworksManaged.Api;

// Native interop struct matching C++ layout (pointers valid only during the call)
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct ConVarInfoNative
{
	public byte* Name;
	public byte* TypeName;
	public byte* Value;
	public byte* DefaultValue;
	public byte* Description;
	public ulong Flags;
	public byte* MinValue;   // null when absent
	public byte* MaxValue;   // null when absent
}
