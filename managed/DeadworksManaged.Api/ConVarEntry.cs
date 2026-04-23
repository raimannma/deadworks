namespace DeadworksManaged.Api;

public record ConVarEntry(
	string Name, string Type, string Value, string DefaultValue,
	string Description, ulong Flags, string? Min, string? Max);
