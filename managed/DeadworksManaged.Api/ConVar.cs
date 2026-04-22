namespace DeadworksManaged.Api;

/// <summary>Wraps a Source 2 console variable (cvar). Use <see cref="Find"/> to look up existing cvars or <see cref="Create"/> to register new ones.</summary>
public sealed unsafe class ConVar {
	private readonly ulong _handle;

	private ConVar(ulong handle) => _handle = handle;

	public bool IsValid => _handle != 0;

	/// <summary>Looks up an existing ConVar by name. Returns null if not found.</summary>
	public static ConVar? Find(string name) {
		Span<byte> utf8 = Utf8.Encode(name, stackalloc byte[Utf8.Size(name)]);
		fixed (byte* ptr = utf8) {
			ulong handle = NativeInterop.FindConVar(ptr);
			return handle != 0 ? new ConVar(handle) : null;
		}
	}

	/// <summary>Creates a new ConVar registered with the engine. Returns null if creation fails.</summary>
	public static ConVar? Create(string name, string defaultValue, string description = "", bool serverOnly = false) {
		Span<byte> nameUtf8 = Utf8.Encode(name, stackalloc byte[Utf8.Size(name)]);
		Span<byte> defUtf8 = Utf8.Encode(defaultValue, stackalloc byte[Utf8.Size(defaultValue)]);
		Span<byte> descUtf8 = Utf8.Encode(description, stackalloc byte[Utf8.Size(description)]);

		FCVar flags = serverOnly ? FCVar.UserInfo : FCVar.None;

		fixed (byte* namePtr = nameUtf8, defPtr = defUtf8, descPtr = descUtf8) {
			ulong handle = NativeInterop.CreateConVar(namePtr, defPtr, descPtr, (ulong)flags);
			return handle != 0 ? new ConVar(handle) : null;
		}
	}

	/// <summary>Gets the cvar's value as an integer.</summary>
	public int GetInt() => NativeInterop.GetConVarInt(_handle);
	/// <summary>Gets the cvar's value as a float.</summary>
	public float GetFloat() => NativeInterop.GetConVarFloat(_handle);
	/// <summary>Gets the cvar's value as a string.</summary>
	public string GetString() {
		byte* ptr = NativeInterop.GetConVarString(_handle);
		return ptr != null ? System.Runtime.InteropServices.Marshal.PtrToStringUTF8((nint)ptr) ?? "" : "";
	}

	/// <summary>Sets the cvar's value as an integer.</summary>
	public void SetInt(int value) => NativeInterop.SetConVarInt(_handle, value);
	/// <summary>Sets the cvar's value as a float.</summary>
	public void SetFloat(float value) => NativeInterop.SetConVarFloat(_handle, value);
}
