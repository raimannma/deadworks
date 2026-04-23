using System.Runtime.InteropServices;

namespace DeadworksManaged.Api;

// Native interop struct matching C++ layout (pointers valid only during the call)
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct ConCommandInfoNative
{
	public byte* Name;
	public byte* Description;
	public ulong Flags;
}
